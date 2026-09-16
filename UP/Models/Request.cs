using System;
using System.Collections.Generic;

namespace BanID.Models;

public partial class Request
{
    public int IdRequest { get; set; }

    public int IdUser { get; set; }

    public int? IdAdmin { get; set; }

    public int? IdAccount { get; set; }

    public int? IdDeposit { get; set; }

    public string TypeRequest { get; set; } = "";

    public string StatusRequest { get; set; } = "";

    public string TextRequest { get; set; } = "";

    public string? CommentRequest { get; set; }

    public DateTime CreatedRequest { get; set; }

    public virtual BankAccount? IdAccountNavigation { get; set; }

    public virtual User? IdAdminNavigation { get; set; }

    public virtual Deposit? IdDepositNavigation { get; set; }

    public virtual User? IdUserNavigation { get; set; }
}
