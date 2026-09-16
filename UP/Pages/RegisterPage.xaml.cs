using System.Windows;
using System.Windows.Controls;

namespace BanID.Pages;

public partial class RegisterPage : Page
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        TBError.Text = "";

        if (TBoxLastName.Text == "" || TBoxFirstName.Text == "" || TBoxPhone.Text == "" || TBoxEmail.Text == "")
        {
            TBError.Text = "Заполните фамилию, имя, телефон и email.";
            return;
        }

        string? error = App.Database.RegisterUser(
            TBoxLastName.Text,
            TBoxFirstName.Text,
            TBoxMiddleName.Text,
            TBoxPhone.Text,
            TBoxEmail.Text,
            PBPassword.Password);

        if (error != null)
        {
            TBError.Text = error;
            return;
        }

        MessageBox.Show("Регистрация завершена.");
        NavigationService.Navigate(new LoginPage());
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        NavigationService.Navigate(new LoginPage());
    }
}

