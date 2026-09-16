using System;
using System.Collections.Generic;

namespace BanID.Models;

public partial class BankAccount
{
    public int IdAccount { get; set; }

    public int IdUser { get; set; }

    public string NumberAccount { get; set; } = "";

    public string TypeAccount { get; set; } = "";

    public decimal BalanceAccount { get; set; }

    public string CurrencyAccount { get; set; } = "";

    public string StatusAccount { get; set; } = "";

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual User? IdUserNavigation { get; set; }

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
