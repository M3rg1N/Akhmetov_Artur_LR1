using System.Windows;
using BanID.Models;
using BanID.Pages;

namespace BanID.Windows;

public partial class DashboardWindow : Window
{
    public DashboardWindow(AuthUser user)
    {
        InitializeComponent();
        TBUserName.Text = user.FullName;
        TBRole.Text = user.IsAdmin ? "Администратор" : "Пользователь";
        FDashboard.Navigate(user.IsAdmin ? new AdminDashboardPage(user) : new UserDashboardPage(user));
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        new MainWindow().Show();
        Close();
    }
}
