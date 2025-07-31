using webapi.DTOs;

namespace webapi.Interfaces;

public interface IGoogleAuthService
{
    Task<AuthResponseDto> LoginWithGoogleAsync(GoogleLoginDto model);
}
