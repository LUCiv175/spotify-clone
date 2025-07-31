using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using webapi.DTOs;
using webapi.Interfaces;
using webapi.Models;

namespace webapi.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ILogger<AuthService> logger,
        IOptions<JwtSettings> jwtOptions
    )
    {
        _userManager = userManager;
        _logger = logger;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            _logger.LogWarning("Login attempt with empty credentials");
            throw new ArgumentException("Email and password are required");
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt for non-existent user: {Email}", model.Email);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        if (!await _userManager.CheckPasswordAsync(user, model.Password))
        {
            _logger.LogWarning("Failed login attempt for user: {Email}", model.Email);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        _logger.LogInformation("Successful login for user: {Email}", model.Email);
        return await GenerateTokenAsync(user);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
    {
        // Validazioni
        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        if (await _userManager.FindByNameAsync(model.Username) != null)
        {
            throw new InvalidOperationException("Username is already taken");
        }

        // Validazione età per artisti
        if (model.RegisterAsArtist && model.Birthdate.HasValue)
        {
            var age = DateTime.UtcNow.Year - model.Birthdate.Value.Year;
            if (age < 16)
            {
                throw new InvalidOperationException("Artists must be at least 16 years old");
            }
        }

        var user = new ApplicationUser
        {
            UserName = model.Username,
            Email = model.Email,
            Name = model.Name,
            Surname = model.Surname,
            Birthdate = model.Birthdate,
            CreatedAt = DateTime.UtcNow,
            ArtistBio = model.RegisterAsArtist ? model.ArtistBio : null,
            IsVerifiedArtist = false,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Registration failed for {Email}: {Errors}", model.Email, errors);
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        // Assegna ruoli
        var rolesToAssign = new List<string> { AppRoles.User };
        if (model.RegisterAsArtist)
        {
            rolesToAssign.Add(AppRoles.Artist);
        }

        foreach (var role in rolesToAssign)
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        _logger.LogInformation(
            "User registered successfully: {Email} with roles: {Roles}",
            model.Email,
            string.Join(", ", rolesToAssign)
        );

        return await GenerateTokenAsync(user);
    }

    // Rendi pubblico il metodo GenerateTokenAsync
    public async Task<AuthResponseDto> GenerateTokenAsync(ApplicationUser user)
    {
        var authClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.GivenName, user.Name),
            new(ClaimTypes.Surname, user.Surname),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(
                JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64
            ),
        };

        // Aggiungi ruoli
        var userRoles = await _userManager.GetRolesAsync(user);
        authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Claims personalizzati per artisti
        if (userRoles.Contains(AppRoles.Artist))
        {
            authClaims.Add(
                new Claim("is_verified_artist", user.IsVerifiedArtist.ToString().ToLower())
            );
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: authClaims,
            expires: expires,
            signingCredentials: creds
        );

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = "", // TODO: implementare refresh token
            Expires = expires,
            User = new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                Name = user.Name,
                Surname = user.Surname,
                Birthdate = user.Birthdate,
                Roles = userRoles.ToArray(),
                IsVerifiedArtist = user.IsVerifiedArtist,
                ArtistBio = user.ArtistBio,
                CreatedAt = user.CreatedAt,
            },
        };
    }
}
