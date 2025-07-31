namespace webapi.DTOs;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public DateTime? Birthdate { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
    public bool IsVerifiedArtist { get; set; }
    public string? ArtistBio { get; set; }
    public DateTime CreatedAt { get; set; }
}
