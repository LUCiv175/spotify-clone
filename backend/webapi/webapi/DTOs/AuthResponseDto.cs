namespace webapi.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public UserProfileDto User { get; set; } = null!;
}
