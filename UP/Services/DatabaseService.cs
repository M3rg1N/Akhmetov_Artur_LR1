using System.IO;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using BanID.Models;

namespace BanID.Services;

public class DatabaseService
{
    static DatabaseService()
    {
        GlobalFontSettings.UseWindowsFontsUnderWindows = true;
    }

    public AuthUser? Authenticate(string login, string password)
    {
        string loginHash = SecurityService.HashLogin(login);
        User? user = App.Context.Users.FirstOrDefault(x => x.HashLoginUser == loginHash && x.IsActiveUser);

        if (user == null)
        {
            string normalizedPhone = SecurityService.NormalizePhone(login);

            if (normalizedPhone.Length == 11)
            {
                string phoneHash = SecurityService.HashLogin(normalizedPhone);
                user = App.Context.Users.FirstOrDefault(x => x.HashLoginUser == phoneHash && x.IsActiveUser);
            }
        }

        if (user == null)
        {
            return null;
        }

        UserDatum? data = App.Context.UserData.FirstOrDefault(x => x.IdUser == user.IdUser);

        if (data == null)
        {
            return null;
        }

        if (SecurityService.HashPassword(password, user.SaltPassUser) != user.HashPassUser)
        {
            return null;
        }

        string salt = data.SaltData;

        AuthUser authUser = new AuthUser
        {
            ID_User = user.IdUser,
            RoleUser = user.RoleUser,
            LastName = SecurityService.DecryptPersonalData(data.LastNameData, salt),
            FirstName = SecurityService.DecryptPersonalData(data.FirstNameData, salt),
            MiddleName = SecurityService.DecryptPersonalData(data.MiddleNameData ?? "", salt),
            Phone = SecurityService.DecryptPersonalData(data.PhoneData, salt),
            Email = SecurityService.DecryptPersonalData(data.EmailData, salt)
        };

        App.CurrentUser = authUser;

        return authUser;
    }

    public string? RegisterUser(string lastName, string firstName, string middleName, string phone, string email, string password)
    {
        if (!SecurityService.IsValidPassword(password, out string error))
        {
            return error;
        }

        string normalizedPhone = SecurityService.NormalizePhone(phone);

        if (normalizedPhone.Length != 11)
        {
            return "Телефон должен содержать 11 цифр.";
        }

        string phoneHash = SecurityService.HashLogin(normalizedPhone);

        if (App.Context.Users.Any(x => x.HashLoginUser == phoneHash))
        {
            return "Пользователь с таким телефоном уже зарегистрирован.";
        }

        User user = CreateUser("User", normalizedPhone, password, lastName, firstName, middleName, email);
        AddAccount(user.IdUser, GenerateNumber(user.IdUser), 0m, "Current");

        return null;
    }

    public List<AccountOption> GetUserAccounts(int userId)
    {
        List<AccountOption> accounts = App.Context.BankAccounts
            .Where(x => x.IdUser == userId && x.StatusAccount == "Active")
            .OrderBy(x => x.IdAccount)
            .Select(x => new AccountOption
            {
                ID_Account = x.IdAccount,
                Number = x.NumberAccount,
                Type = x.TypeAccount,
                Balance = x.BalanceAccount,
                Currency = x.CurrencyAccount
            })
            .ToList();

        foreach (AccountOption account in accounts)
        {
            account.HasCloseRequest = HasNewRequest(userId, "CLOSE_ACCOUNT", account.ID_Account, null);
        }

        return accounts;
    }

    public List<DepositItem> GetUserDeposits(int userId)
    {
        List<DepositItem> deposits = App.Context.Deposits
            .Where(x => x.IdUser == userId)
            .OrderBy(x => x.IdDeposit)
            .Select(x => new DepositItem
            {
                ID_Deposit = x.IdDeposit,
                ID_Account = x.IdAccount,
                DepositName = x.NameDeposit,
                Amount = x.AmountDeposit,
                InterestRate = x.RateDeposit,
                MaturityDate = x.EndDeposit.ToDateTime(TimeOnly.MinValue),
                Status = x.StatusDeposit
            })
            .ToList();

        foreach (DepositItem deposit in deposits)
        {
            deposit.HasCloseRequest = HasNewRequest(userId, "CLOSE_DEPOSIT", deposit.ID_Account, deposit.ID_Deposit);
        }

        return deposits;
    }

    public List<UserRequestItem> GetRequests(int userId)
    {
        return App.Context.Requests
            .Where(x => x.IdUser == userId)
            .OrderByDescending(x => x.CreatedRequest)
            .Select(x => new UserRequestItem
            {
                ID_Request = x.IdRequest,
                Name = GetRequestTypeName(x.TypeRequest),
                Status = x.StatusRequest,
                Text = x.TextRequest,
                Comment = x.CommentRequest ?? "",
                CreatedAt = x.CreatedRequest
            })
            .ToList();
    }

    public List<AdminRequestItem> GetAdminRequests()
    {
        List<Request> requests = App.Context.Requests
            .OrderByDescending(x => x.CreatedRequest)
            .ToList();

        List<AdminRequestItem> items = new List<AdminRequestItem>();

        foreach (Request request in requests)
        {
            UserDatum? data = App.Context.UserData.FirstOrDefault(x => x.IdUser == request.IdUser);
            string clientName = "";

            if (data != null)
            {
                string salt = data.SaltData;
                clientName =
                    SecurityService.DecryptPersonalData(data.LastNameData, salt) + " " +
                    SecurityService.DecryptPersonalData(data.FirstNameData, salt) + " " +
                    SecurityService.DecryptPersonalData(data.MiddleNameData ?? "", salt);
            }

            items.Add(new AdminRequestItem
            {
                ID_Request = request.IdRequest,
                Name = GetRequestTypeName(request.TypeRequest),
                Status = request.StatusRequest,
                ClientName = clientName.Trim(),
                Text = request.TextRequest,
                CreatedAt = request.CreatedRequest
            });
        }

        return items;
    }

    public void AddUserRequest(int userId, string typeCode, string text, int? accountId, int? depositId)
    {
        if (HasNewRequest(userId, typeCode, accountId, depositId))
        {
            return;
        }

        Request request = new Request
        {
            IdUser = userId,
            IdAccount = accountId,
            IdDeposit = depositId,
            TypeRequest = typeCode,
            StatusRequest = "New",
            TextRequest = text,
            CreatedRequest = DateTime.Now
        };

        App.Context.Requests.Add(request);
        App.Context.SaveChanges();
    }

    public string AddStatement(int userId, int accountId)
    {
        string number = "ST-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        string folder = "Statements";
        Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, number + ".pdf");
        string text = GetStatementText(userId, accountId, number);
        SavePdf(path, text);

        return path;
    }

    public void ReviewRequest(int adminId, int requestId, bool approved)
    {
        Request? request = App.Context.Requests.FirstOrDefault(x => x.IdRequest == requestId);

        if (request == null)
        {
            throw new Exception("Заявка не найдена.");
        }

        if (request.StatusRequest != "New")
        {
            throw new Exception("Эта заявка уже обработана.");
        }

        if (approved)
        {
            ApplyRequest(request);
        }

        request.StatusRequest = approved ? "Approved" : "Rejected";
        request.CommentRequest = approved ? "Заявка одобрена. Действие выполнено." : "Заявка отклонена.";
        request.IdAdmin = adminId;

        App.Context.SaveChanges();
    }

    private bool HasNewRequest(int userId, string typeCode, int? accountId, int? depositId)
    {
        return App.Context.Requests.Any(x =>
            x.IdUser == userId &&
            x.StatusRequest == "New" &&
            x.TypeRequest == typeCode &&
            x.IdAccount == accountId &&
            x.IdDeposit == depositId);
    }

    private void ApplyRequest(Request request)
    {
        if (request.TypeRequest == "OPEN_ACCOUNT")
        {
            AddAccount(request.IdUser, GenerateNumber(request.IdUser), 0m, "Current");
        }

        if (request.TypeRequest == "OPEN_DEPOSIT")
        {
            int accountId = request.IdAccount ?? GetFirstAccountId(request.IdUser);
            AddDeposit(request.IdUser, accountId, "Новый вклад", 10000m, 8.5m);
        }

        if (request.TypeRequest == "CHANGE_PERSONAL_DATA")
        {
            ChangePersonalData(request);
        }

        if (request.TypeRequest == "CLOSE_ACCOUNT")
        {
            int accountId = request.IdAccount ?? GetFirstAccountId(request.IdUser);
            BankAccount? account = App.Context.BankAccounts.FirstOrDefault(x => x.IdAccount == accountId);

            if (account != null)
            {
                account.StatusAccount = "Closed";
            }
        }

        if (request.TypeRequest == "CLOSE_DEPOSIT")
        {
            int? depositId = request.IdDeposit;

            if (depositId == null && request.IdAccount != null)
            {
                depositId = App.Context.Deposits
                    .Where(x => x.IdAccount == request.IdAccount && x.IdUser == request.IdUser && x.StatusDeposit == "Active")
                    .Select(x => (int?)x.IdDeposit)
                    .FirstOrDefault();
            }

            if (depositId != null)
            {
                Deposit? deposit = App.Context.Deposits.FirstOrDefault(x => x.IdDeposit == depositId);

                if (deposit != null)
                {
                    deposit.StatusDeposit = "Closed";
                }
            }
        }
    }

    private void ChangePersonalData(Request request)
    {
        User? user = App.Context.Users.FirstOrDefault(x => x.IdUser == request.IdUser);
        UserDatum? data = App.Context.UserData.FirstOrDefault(x => x.IdUser == request.IdUser);

        if (user == null || data == null)
        {
            return;
        }

        string salt = data.SaltData;
        string lastName = GetChangeValue(request.TextRequest, "Фамилия");
        string firstName = GetChangeValue(request.TextRequest, "Имя");
        string middleName = GetChangeValue(request.TextRequest, "Отчество");
        string phone = GetChangeValue(request.TextRequest, "Телефон");
        string email = GetChangeValue(request.TextRequest, "Email");

        if (lastName != "")
        {
            data.LastNameData = SecurityService.EncryptPersonalData(lastName, salt);
        }

        if (firstName != "")
        {
            data.FirstNameData = SecurityService.EncryptPersonalData(firstName, salt);
        }

        if (middleName != "")
        {
            data.MiddleNameData = SecurityService.EncryptPersonalData(middleName, salt);
        }

        if (phone != "")
        {
            phone = SecurityService.NormalizePhone(phone);

            if (phone.Length != 11)
            {
                throw new Exception("Телефон должен содержать 11 цифр.");
            }

            string phoneHash = SecurityService.HashLogin(phone);
            bool phoneExists = App.Context.Users.Any(x => x.HashLoginUser == phoneHash && x.IdUser != request.IdUser);

            if (phoneExists)
            {
                throw new Exception("Такой телефон уже используется другим пользователем.");
            }

            data.PhoneData = SecurityService.EncryptPersonalData(phone, salt);
            user.HashLoginUser = phoneHash;
        }

        if (email != "")
        {
            data.EmailData = SecurityService.EncryptPersonalData(email, salt);
        }
    }

    private string GetChangeValue(string text, string name)
    {
        string[] lines = text.Split('\n');

        foreach (string sourceLine in lines)
        {
            string line = sourceLine.Trim();

            if (line.StartsWith(name + ":"))
            {
                return line.Substring(name.Length + 1).Trim();
            }

            if (line.StartsWith(name + "="))
            {
                return line.Substring(name.Length + 1).Trim();
            }
        }

        return "";
    }

    private User CreateUser(string roleName, string phone, string password, string lastName, string firstName, string middleName, string email)
    {
        string passwordSalt = SecurityService.GenerateSalt();
        string dataSalt = SecurityService.GenerateSalt();

        User user = new User
        {
            RoleUser = roleName,
            HashLoginUser = SecurityService.HashLogin(phone),
            HashPassUser = SecurityService.HashPassword(password, passwordSalt),
            SaltPassUser = passwordSalt,
            IsActiveUser = true
        };

        App.Context.Users.Add(user);
        App.Context.SaveChanges();

        UserDatum data = new UserDatum
        {
            IdUser = user.IdUser,
            LastNameData = SecurityService.EncryptPersonalData(lastName, dataSalt),
            FirstNameData = SecurityService.EncryptPersonalData(firstName, dataSalt),
            MiddleNameData = SecurityService.EncryptPersonalData(middleName, dataSalt),
            PhoneData = SecurityService.EncryptPersonalData(phone, dataSalt),
            EmailData = SecurityService.EncryptPersonalData(email, dataSalt),
            SaltData = dataSalt
        };

        App.Context.UserData.Add(data);
        App.Context.SaveChanges();

        return user;
    }

    private BankAccount AddAccount(int userId, string number, decimal balance, string type)
    {
        BankAccount account = new BankAccount
        {
            IdUser = userId,
            NumberAccount = number,
            CurrencyAccount = "RUB",
            BalanceAccount = balance,
            TypeAccount = type,
            StatusAccount = "Active"
        };

        App.Context.BankAccounts.Add(account);
        App.Context.SaveChanges();

        return account;
    }

    private void AddDeposit(int userId, int accountId, string name, decimal amount, decimal rate)
    {
        Deposit deposit = new Deposit
        {
            IdUser = userId,
            IdAccount = accountId,
            NameDeposit = name,
            AmountDeposit = amount,
            RateDeposit = rate,
            EndDeposit = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            StatusDeposit = "Active"
        };

        App.Context.Deposits.Add(deposit);
        App.Context.SaveChanges();
    }

    private int GetFirstAccountId(int userId)
    {
        BankAccount? account = App.Context.BankAccounts.FirstOrDefault(x => x.IdUser == userId && x.StatusAccount == "Active");

        if (account == null)
        {
            throw new Exception("У пользователя нет активного счета.");
        }

        return account.IdAccount;
    }

    private string GenerateNumber(int userId)
    {
        Random random = new Random();
        return "40817810" + DateTime.Now.ToString("yyMMddHHmmss") + userId.ToString().PadLeft(6, '0') + random.Next(1000, 9999);
    }

    private string GetStatementText(int userId, int accountId, string number)
    {
        UserDatum? data = App.Context.UserData.FirstOrDefault(x => x.IdUser == userId);
        BankAccount? account = App.Context.BankAccounts.FirstOrDefault(x => x.IdAccount == accountId);

        string client = "";
        string phone = "";
        string email = "";

        if (data != null)
        {
            string salt = data.SaltData;
            client =
                SecurityService.DecryptPersonalData(data.LastNameData, salt) + " " +
                SecurityService.DecryptPersonalData(data.FirstNameData, salt) + " " +
                SecurityService.DecryptPersonalData(data.MiddleNameData ?? "", salt);
            phone = SecurityService.DecryptPersonalData(data.PhoneData, salt);
            email = SecurityService.DecryptPersonalData(data.EmailData, salt);
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("BanID");
        builder.AppendLine("Детальная выписка по счету");
        builder.AppendLine("------------------------------------------");
        builder.AppendLine("Номер документа: " + number);
        builder.AppendLine("Дата: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
        builder.AppendLine("Клиент: " + client.Trim());
        builder.AppendLine("Телефон: " + phone);
        builder.AppendLine("Email: " + email);
        builder.AppendLine();

        if (account != null)
        {
            builder.AppendLine("Счет: " + account.NumberAccount);
            builder.AppendLine("Тип счета: " + account.TypeAccount);
            builder.AppendLine("Баланс: " + account.BalanceAccount.ToString("N2") + " " + account.CurrencyAccount);
        }

        builder.AppendLine();
        builder.AppendLine("Документ сформирован автоматически и доступен для печати.");

        return builder.ToString();
    }

    private void SavePdf(string path, string text)
    {
        PdfDocument document = new PdfDocument();
        PdfPage page = document.AddPage();
        XGraphics graphics = XGraphics.FromPdfPage(page);
        XTextFormatter formatter = new XTextFormatter(graphics);
        XFont font = new XFont(
            "Arial",
            12,
            XFontStyleEx.Regular,
            new XPdfFontOptions(PdfFontEncoding.Unicode));

        formatter.DrawString(text, font, XBrushes.Black, new XRect(40, 40, page.Width.Point - 80, page.Height.Point - 80));

        document.Save(path);
        document.Close();
    }

    private static string GetRequestTypeName(string typeCode)
    {
        if (typeCode == "OPEN_ACCOUNT") return "Открытие счета";
        if (typeCode == "OPEN_DEPOSIT") return "Открытие вклада";
        if (typeCode == "CHANGE_PERSONAL_DATA") return "Смена персональных данных";
        if (typeCode == "CLOSE_ACCOUNT") return "Закрытие счета";
        if (typeCode == "CLOSE_DEPOSIT") return "Закрытие вклада";
        return typeCode;
    }
}
