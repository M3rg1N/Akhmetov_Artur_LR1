using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using BanID.Models;

namespace BanID.Pages;

public partial class UserDashboardPage : Page
{
    private AuthUser _user;

    public UserDashboardPage(AuthUser authUser)
    {
        InitializeComponent();
        _user = authUser;
        TBPersonalData.Text = "ФИО: " + _user.FullName + "\nТелефон: " + _user.Phone + "\nEmail: " + _user.Email;
        LoadItems();
    }

    private void CloseAccount_Click(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        int accountId = (int)button.Tag;
        CreateRequest("CLOSE_ACCOUNT", "Прошу закрыть выбранный счет.", accountId, null);
    }

    private void CloseDeposit_Click(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        int depositId = (int)button.Tag;
        DepositItem? deposit = button.DataContext as DepositItem;

        if (deposit == null)
        {
            return;
        }

        CreateRequest("CLOSE_DEPOSIT", "Прошу закрыть выбранный вклад.", deposit.ID_Account, depositId);
    }

    private void CreateRequest_Click(object sender, RoutedEventArgs e)
    {
        ComboBoxItem item = (ComboBoxItem)CBRequestType.SelectedItem;
        string typeCode = (string)item.Tag;
        string text = TBoxRequest.Text;

        if (typeCode == "CHANGE_PERSONAL_DATA")
        {
            text = BuildChangeDataText();

            if (text.Trim() == "")
            {
                MessageBox.Show("Укажите, какие данные нужно изменить.");
                return;
            }

            if (ChBDocument.IsChecked != true)
            {
                MessageBox.Show("Для смены данных нужно приложить фото документа.");
                return;
            }

            text = text.Trim() + "\nДокумент: фото приложено";
        }

        if (text == "")
        {
            text = item.Content?.ToString() ?? "";
        }

        CreateRequest(typeCode, text, null, null);
        TBoxRequest.Text = "";
        ChBDocument.IsChecked = false;
        ClearChangeDataTextBoxes();
    }

    private void RequestType_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (CBRequestType.SelectedItem == null || TBRequestHint == null || ChBDocument == null)
        {
            return;
        }

        ComboBoxItem item = (ComboBoxItem)CBRequestType.SelectedItem;
        string typeCode = (string)item.Tag;

        if (typeCode == "CHANGE_PERSONAL_DATA")
        {
            TBoxRequest.Visibility = Visibility.Collapsed;
            GridChangeData.Visibility = Visibility.Visible;
            ChBDocument.Visibility = Visibility.Visible;
            TBRequestHint.Text = "Заполните только те поля, которые нужно изменить.";
        }
        else
        {
            TBoxRequest.Visibility = Visibility.Visible;
            GridChangeData.Visibility = Visibility.Collapsed;
            ChBDocument.Visibility = Visibility.Collapsed;
            TBRequestHint.Text = "";
        }
    }

    private void CreateStatement_Click(object sender, RoutedEventArgs e)
    {
        if (CBStatementAccount.SelectedItem == null)
        {
            MessageBox.Show("Выберите счет.");
            return;
        }

        AccountOption account = (AccountOption)CBStatementAccount.SelectedItem;
        string path = App.Database.AddStatement(_user.ID_User, account.ID_Account);
        MessageBox.Show("PDF-выписка сохранена:\n" + path);
        OpenFile(path);
    }

    private string BuildChangeDataText()
    {
        List<string> lines = new List<string>();

        if (TBoxChangeLastName.Text.Trim() != "") lines.Add("Фамилия: " + TBoxChangeLastName.Text.Trim());
        if (TBoxChangeFirstName.Text.Trim() != "") lines.Add("Имя: " + TBoxChangeFirstName.Text.Trim());
        if (TBoxChangeMiddleName.Text.Trim() != "") lines.Add("Отчество: " + TBoxChangeMiddleName.Text.Trim());
        if (TBoxChangePhone.Text.Trim() != "") lines.Add("Телефон: " + TBoxChangePhone.Text.Trim());
        if (TBoxChangeEmail.Text.Trim() != "") lines.Add("Email: " + TBoxChangeEmail.Text.Trim());

        return string.Join("\n", lines);
    }

    private void ClearChangeDataTextBoxes()
    {
        TBoxChangeLastName.Text = "";
        TBoxChangeFirstName.Text = "";
        TBoxChangeMiddleName.Text = "";
        TBoxChangePhone.Text = "";
        TBoxChangeEmail.Text = "";
    }

    private void CreateRequest(string typeCode, string text, int? accountId, int? depositId)
    {
        App.Database.AddUserRequest(_user.ID_User, typeCode, text, accountId, depositId);
        MessageBox.Show("Заявка отправлена администратору.");
        LoadItems();
    }

    private void LoadItems()
    {
        List<AccountOption> accounts = App.Database.GetUserAccounts(_user.ID_User);

        LBAccounts.ItemsSource = accounts;
        LBDeposits.ItemsSource = App.Database.GetUserDeposits(_user.ID_User);
        LBRequests.ItemsSource = App.Database.GetRequests(_user.ID_User);
        CBStatementAccount.ItemsSource = accounts;

        if (CBStatementAccount.SelectedIndex == -1 && accounts.Count > 0)
        {
            CBStatementAccount.SelectedIndex = 0;
        }
    }

    private void OpenFile(string path)
    {
        if (!File.Exists(path))
        {
            MessageBox.Show("Файл не найден.");
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }
}
