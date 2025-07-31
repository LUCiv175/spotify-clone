using webapi.DTOs;
using webapi.Models;

namespace webapi.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto model, string? ipAddress = null);
    Task<AuthResponseDto> RegisterAsync(RegisterDto model, string? ipAddress = null);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null); // 🆕 Nuovo
    Task RevokeTokenAsync(string refreshToken, string? ipAddress = null); // 🆕 Nuovo
    Task<AuthResponseDto> GenerateTokenAsync(ApplicationUser user, string? ipAddress = null);
}
