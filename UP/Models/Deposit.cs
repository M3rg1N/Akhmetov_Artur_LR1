using System;
using System.Collections.Generic;

namespace BanID.Models;

public partial class Deposit
{
    public int IdDeposit { get; set; }

    public int IdUser { get; set; }

    public int IdAccount { get; set; }

    public string NameDeposit { get; set; } = "";

    public decimal AmountDeposit { get; set; }

    public decimal RateDeposit { get; set; }

    public DateOnly EndDeposit { get; set; }

    public string StatusDeposit { get; set; } = "";

    public virtual BankAccount? IdAccountNavigation { get; set; }

    public virtual User? IdUserNavigation { get; set; }

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
