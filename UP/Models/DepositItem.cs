namespace BanID.Models;

public sealed class DepositItem
{
    public int ID_Deposit { get; set; }

    public int ID_Account { get; set; }

    public string DepositName { get; set; } = "";

    public decimal Amount { get; set; }

    public decimal InterestRate { get; set; }

    public DateTime MaturityDate { get; set; }

    public string Status { get; set; } = "";

    public bool HasCloseRequest { get; set; }

    public string Title => DepositName;

    public string StatusText
    {
        get
        {
            if (Status == "Active") return "Активен";
            if (Status == "Closed") return "Закрыт";
            return Status;
        }
    }

    public string Description => $"Сумма: {Amount:N2}\nСтавка: {InterestRate:N2}%\nДо: {MaturityDate:dd.MM.yyyy}\nСтатус: {StatusText}";
    public bool CanClose => Status == "Active";
    public string CloseButtonText => HasCloseRequest ? "В обработке" : "Закрыть вклад";
    public string CloseButtonColor => HasCloseRequest ? "#2F6FA3" : "#DDE8E4";
    public string CloseButtonTextColor => HasCloseRequest ? "White" : "#21312D";
    public bool CanSendCloseRequest => !HasCloseRequest;
}
