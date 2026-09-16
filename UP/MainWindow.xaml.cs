using System.Windows;
using BanID.Pages;

namespace BanID;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        FAuth.Navigate(new LoginPage());
    }
}
