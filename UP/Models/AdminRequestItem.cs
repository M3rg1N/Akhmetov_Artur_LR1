namespace BanID.Models;

public sealed class AdminRequestItem
{
    public int ID_Request { get; set; }
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public string StatusText
    {
        get
        {
            if (Status == "New") return "На рассмотрении";
            if (Status == "Approved") return "Принята";
            if (Status == "Rejected") return "Отклонена";
            return Status;
        }
    }

    public string Title => $"#{ID_Request} {Name} - {StatusText}";
    public string Description => ClientName + "\n" + Text + "\nСоздана: " + CreatedAt.ToString("dd.MM.yyyy HH:mm");
    public bool CanReview => Status == "New";
    public string SearchText => (ID_Request + " " + Name + " " + Status + " " + StatusText + " " + ClientName + " " + Text).ToLower();
}
