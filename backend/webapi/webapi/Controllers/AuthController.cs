using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DTOs;
using webapi.Interfaces;

namespace webapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IGoogleAuthService googleAuthService)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto model)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var result = await authService.LoginAsync(model, ipAddress);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto model)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var result = await authService.RegisterAsync(model, ipAddress);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin(GoogleLoginDto model)
    {
        try
        {
            var result = await googleAuthService.LoginWithGoogleAsync(model);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Refresh user token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"] ?? "";
            var ipAddress = GetIpAddress();

            var result = await authService.RefreshTokenAsync(refreshToken, ipAddress);
            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Revoke user token
    /// </summary>
    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken(RefreshTokenDto? model = null)
    {
        try
        {
            var refreshToken = model?.RefreshToken ?? Request.Cookies["refreshToken"] ?? "";
            var ipAddress = GetIpAddress();

            await authService.RevokeTokenAsync(refreshToken, ipAddress);
            return Ok(new { message = "Token revoked successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public IActionResult GetProfile()
    {
        return Ok(
            new
            {
                UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                Roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value),
            }
        );
    }

    // 🔧 Helper Methods

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            Secure = true, // Solo HTTPS in produzione
            SameSite = SameSiteMode.Strict,
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private string GetIpAddress()
    {
        return Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim()
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
    }
}
