using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BanID.Models;
using BanID.Windows;

namespace BanID.Pages;

public partial class LoginPage : Page
{
    private const string CaptchaSymbols = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private string _captchaText = "";

    public LoginPage()
    {
        InitializeComponent();
        GenerateCaptcha();
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        TBError.Text = "";

        if (TBoxCaptcha.Text.Trim().ToUpper() != _captchaText)
        {
            TBError.Text = "Капча введена неверно.";
            GenerateCaptcha();
            return;
        }

        AuthUser? user = App.Database.Authenticate(TBoxLogin.Text, PBPassword.Password);

        if (user == null)
        {
            TBError.Text = "Неверный логин или пароль.";
            GenerateCaptcha();
            return;
        }

        DashboardWindow window = new DashboardWindow(user);
        window.Show();
        Window.GetWindow(this).Close();
    }

    private void OpenRegister_Click(object sender, RoutedEventArgs e)
    {
        NavigationService.Navigate(new RegisterPage());
    }

    private void RefreshCaptcha_Click(object sender, RoutedEventArgs e)
    {
        GenerateCaptcha();
    }

    private void GenerateCaptcha()
    {
        Random random = new Random();
        char[] chars = new char[5];

        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = CaptchaSymbols[random.Next(CaptchaSymbols.Length)];
        }

        _captchaText = new string(chars);
        CaptchaCanvas.Children.Clear();
        DrawNoise(random);
        DrawCaptchaText(random);
        DrawLines(random);
        TBoxCaptcha.Text = "";
    }

    private void DrawCaptchaText(Random random)
    {
        for (int i = 0; i < _captchaText.Length; i++)
        {
            TextBlock symbol = new TextBlock
            {
                Text = _captchaText[i].ToString(),
                FontSize = random.Next(24, 31),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(
                    (byte)random.Next(30, 110),
                    (byte)random.Next(60, 130),
                    (byte)random.Next(50, 120)))
            };

            symbol.RenderTransform = new RotateTransform(random.Next(-25, 26));
            Canvas.SetLeft(symbol, 14 + (i * 31) + random.Next(-3, 4));
            Canvas.SetTop(symbol, 10 + random.Next(-4, 5));
            CaptchaCanvas.Children.Add(symbol);
        }
    }

    private void DrawLines(Random random)
    {
        for (int i = 0; i < 3; i++)
        {
            Line line = new Line
            {
                X1 = random.Next(0, (int)CaptchaCanvas.Width),
                Y1 = random.Next(0, (int)CaptchaCanvas.Height),
                X2 = random.Next(0, (int)CaptchaCanvas.Width),
                Y2 = random.Next(0, (int)CaptchaCanvas.Height),
                StrokeThickness = random.Next(1, 3),
                Stroke = new SolidColorBrush(Color.FromArgb(
                    180,
                    (byte)random.Next(90, 170),
                    (byte)random.Next(90, 170),
                    (byte)random.Next(90, 170)))
            };

            CaptchaCanvas.Children.Add(line);
        }
    }

    private void DrawNoise(Random random)
    {
        for (int i = 0; i < 24; i++)
        {
            Ellipse dot = new Ellipse
            {
                Width = random.Next(2, 5),
                Height = random.Next(2, 5),
                Fill = new SolidColorBrush(Color.FromArgb(
                    140,
                    (byte)random.Next(110, 190),
                    (byte)random.Next(110, 190),
                    (byte)random.Next(110, 190)))
            };

            Canvas.SetLeft(dot, random.Next(0, (int)CaptchaCanvas.Width));
            Canvas.SetTop(dot, random.Next(0, (int)CaptchaCanvas.Height));
            CaptchaCanvas.Children.Add(dot);
        }
    }
}
