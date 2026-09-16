namespace BanID.Models;

public sealed class AccountOption
{
    public int ID_Account { get; set; }
    public string Number { get; set; } = "";
    public string Type { get; set; } = "";
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "";
    public bool HasCloseRequest { get; set; }

    public string TypeText
    {
        get
        {
            if (Type == "Current") return "Текущий счет";
            if (Type == "Savings") return "Накопительный счет";
            return Type;
        }
    }

    public string DisplayText => $"{TypeText} - {Balance:N2} {Currency}";
    public string CloseButtonText => HasCloseRequest ? "В обработке" : "Заявка на закрытие";
    public string CloseButtonColor => HasCloseRequest ? "#2F6FA3" : "#DDE8E4";
    public string CloseButtonTextColor => HasCloseRequest ? "White" : "#21312D";
    public bool CanSendCloseRequest => !HasCloseRequest;
}
