using webapi.DTOs;

namespace webapi.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto model);
    Task<AuthResponseDto> RegisterAsync(RegisterDto model);
}