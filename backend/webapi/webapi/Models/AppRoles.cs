namespace webapi.Models;

public static class AppRoles
{
    public const string User = "User";
    public const string Artist = "Artist";
    public const string Admin = "Admin";

    public static readonly string[] All = { User, Artist, Admin };
}
