// Updated GoogleAuthService without the problematic cast
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using webapi.DTOs;
using webapi.Interfaces;
using webapi.Models;

namespace webapi.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;
        private readonly ILogger<GoogleAuthService> _logger;
        private readonly string _googleClientId;

        public GoogleAuthService(
            UserManager<ApplicationUser> userManager,
            IAuthService authService,
            ILogger<GoogleAuthService> logger,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _authService = authService;
            _logger = logger;
            _googleClientId =
                Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
                ?? throw new InvalidOperationException("GOOGLE_CLIENT_ID is required");
        }

        public async Task<AuthResponseDto> LoginWithGoogleAsync(GoogleLoginDto model)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    model.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _googleClientId },
                    }
                );
                var user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = payload.Email,
                        Email = payload.Email,
                        Name = payload.GivenName ?? "",
                        Surname = payload.FamilyName ?? "",
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to create user: {errors}");
                    }

                    await _userManager.AddToRoleAsync(user, AppRoles.User);

                    _logger.LogInformation("New user created via Google: {Email}", payload.Email);
                }
                else
                {
                    _logger.LogInformation(
                        "Existing user logged in via Google: {Email}",
                        payload.Email
                    );
                }
                return await _authService.GenerateTokenAsync(user);
            }
            catch (InvalidJwtException)
            {
                throw new UnauthorizedAccessException("Invalid Google token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google authentication");
                throw;
            }
        }
    }
}
