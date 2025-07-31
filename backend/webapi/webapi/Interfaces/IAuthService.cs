using webapi.DTOs;
using webapi.Models;

namespace webapi.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto model);
    Task<AuthResponseDto> RegisterAsync(RegisterDto model);
    Task<AuthResponseDto> GenerateTokenAsync(ApplicationUser user); // 🆕 Nuovo
}
