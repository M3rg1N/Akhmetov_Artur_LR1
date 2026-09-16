using System.Windows;
using BanID.Models;
using BanID.Services;

namespace BanID;

public partial class App : Application
{
    public static BanIdContext Context { get; } = new BanIdContext();
    public static DatabaseService Database { get; } = new DatabaseService();
    public static AuthUser? CurrentUser { get; set; }
}
