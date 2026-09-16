namespace BanID.Models;

public sealed class AuthUser
{
    public int ID_User { get; set; }

    public string RoleUser { get; set; } = "";

    public string LastName { get; set; } = "";

    public string FirstName { get; set; } = "";

    public string MiddleName { get; set; } = "";

    public string Phone { get; set; } = "";

    public string Email { get; set; } = "";

    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    public bool IsAdmin => RoleUser == "Admin";
}

