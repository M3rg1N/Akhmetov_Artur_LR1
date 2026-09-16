using System;
using System.Collections.Generic;

namespace BanID.Models;

public partial class UserDatum
{
    public int IdData { get; set; }

    public int IdUser { get; set; }

    public string LastNameData { get; set; } = "";

    public string FirstNameData { get; set; } = "";

    public string? MiddleNameData { get; set; }

    public string PhoneData { get; set; } = "";

    public string EmailData { get; set; } = "";

    public string SaltData { get; set; } = "";

    public virtual User? IdUserNavigation { get; set; }
}
