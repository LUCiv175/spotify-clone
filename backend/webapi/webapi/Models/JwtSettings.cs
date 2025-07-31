namespace webapi.Models;

public class JwtSettings
{
    public required string SecretKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int ExpiresInMinutes { get; set; } = 60; // Default to 60 minutes
    public int RefreshExpiresInDays { get; set; } = 7; // Default to 7 days
}
