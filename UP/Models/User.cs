using System;
using System.Collections.Generic;

namespace BanID.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string RoleUser { get; set; } = "";

    public string HashLoginUser { get; set; } = "";

    public string HashPassUser { get; set; } = "";

    public string SaltPassUser { get; set; } = "";

    public bool IsActiveUser { get; set; }

    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<Request> RequestIdAdminNavigations { get; set; } = new List<Request>();

    public virtual ICollection<Request> RequestIdUserNavigations { get; set; } = new List<Request>();

    public virtual UserDatum? UserDatum { get; set; }
}
