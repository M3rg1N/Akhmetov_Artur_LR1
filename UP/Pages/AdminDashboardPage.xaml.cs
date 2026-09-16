using System.Windows;
using System.Windows.Controls;
using BanID.Models;

namespace BanID.Pages;

public partial class AdminDashboardPage : Page
{
    private AuthUser _admin;
    private List<AdminRequestItem> _requests = new List<AdminRequestItem>();

    public AdminDashboardPage(AuthUser authUser)
    {
        InitializeComponent();
        _admin = authUser;
        LoadRequests();
    }

    private void ApproveRequest_Click(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        Review((int)button.Tag, true);
    }

    private void RejectRequest_Click(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        Review((int)button.Tag, false);
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadRequests();
    }

    private void Search_TextChanged(object sender, TextChangedEventArgs e)
    {
        ShowRequests();
    }

    private void RequestsTab_Changed(object sender, SelectionChangedEventArgs e)
    {
        ShowRequests();
    }

    private void Review(int id, bool approved)
    {
        try
        {
            App.Database.ReviewRequest(_admin.ID_User, id, approved);
            LoadRequests();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void LoadRequests()
    {
        _requests = App.Database.GetAdminRequests();
        ShowRequests();
    }

    private void ShowRequests()
    {
        string search = TBoxSearch.Text.ToLower();
        List<AdminRequestItem> pending = new List<AdminRequestItem>();
        List<AdminRequestItem> approved = new List<AdminRequestItem>();
        List<AdminRequestItem> rejected = new List<AdminRequestItem>();

        foreach (AdminRequestItem request in _requests)
        {
            if (search != "" && !request.SearchText.Contains(search))
            {
                continue;
            }

            if (request.Status == "New")
            {
                pending.Add(request);
            }

            if (request.Status == "Approved")
            {
                approved.Add(request);
            }

            if (request.Status == "Rejected")
            {
                rejected.Add(request);
            }
        }

        LBPendingRequests.ItemsSource = pending;
        LBApprovedRequests.ItemsSource = approved;
        LBRejectedRequests.ItemsSource = rejected;
    }
}

