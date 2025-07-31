using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Data;
using webapi.DTOs;
using webapi.Interfaces;
using webapi.Models;

namespace webapi.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context,
    IOptions<JwtSettings> jwtOptions,
    ILogger<AuthService> logger
) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<AuthResponseDto> LoginAsync(LoginDto model, string? ipAddress = null)
    {
        var user =
            await userManager.FindByEmailAsync(model.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!await userManager.CheckPasswordAsync(user, model.Password))
            throw new UnauthorizedAccessException("Invalid credentials");

        logger.LogInformation("User {Email} logged in successfully", model.Email);
        return await GenerateTokenAsync(user, ipAddress);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto model, string? ipAddress = null)
    {
        if (await userManager.FindByEmailAsync(model.Email) != null)
            throw new InvalidOperationException("User already exists");

        if (await userManager.FindByNameAsync(model.Username) != null)
            throw new InvalidOperationException("Username taken");

        var user = new ApplicationUser
        {
            UserName = model.Username,
            Email = model.Email,
            Name = model.Name,
            Surname = model.Surname,
            Birthdate = model.Birthdate,
            ArtistBio = model.RegisterAsArtist ? model.ArtistBio : null,
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description))
            );

        // Assign roles
        var roles = new List<string> { AppRoles.User };
        if (model.RegisterAsArtist)
            roles.Add(AppRoles.Artist);

        foreach (var role in roles)
            await userManager.AddToRoleAsync(user, role);

        logger.LogInformation(
            "User {Email} registered successfully with roles: {Roles}",
            model.Email,
            string.Join(", ", roles)
        );

        return await GenerateTokenAsync(user, ipAddress);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        string refreshToken,
        string? ipAddress = null
    )
    {
        var token = await GetRefreshTokenAsync(refreshToken);
        var user = token.User;

        // Revoke current token and generate new one
        await RevokeRefreshTokenAsync(token, ipAddress, "Replaced by new token");
        var newRefreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);

        user.RefreshTokens.Add(newRefreshToken);
        await RemoveOldRefreshTokensAsync(user);
        await context.SaveChangesAsync();

        logger.LogInformation("Refresh token renewed for user {UserId}", user.Id);
        return await GenerateJwtTokenAsync(user, newRefreshToken.Token);
    }

    public async Task RevokeTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var token = await GetRefreshTokenAsync(refreshToken);
        await RevokeRefreshTokenAsync(token, ipAddress, "Revoked by user");
        await context.SaveChangesAsync();

        logger.LogInformation("Refresh token revoked for user {UserId}", token.UserId);
    }

    public async Task<AuthResponseDto> GenerateTokenAsync(
        ApplicationUser user,
        string? ipAddress = null
    )
    {
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);
        user.RefreshTokens.Add(refreshToken);

        await RemoveOldRefreshTokensAsync(user);
        await context.SaveChangesAsync();

        return await GenerateJwtTokenAsync(user, refreshToken.Token);
    }

    // 🔧 Private Methods

    private async Task<AuthResponseDto> GenerateJwtTokenAsync(
        ApplicationUser user,
        string refreshToken
    )
    {
        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(
                JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64
            ),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (roles.Contains(AppRoles.Artist))
            claims.Add(new Claim("is_verified_artist", user.IsVerifiedArtist.ToString().ToLower()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes);
        var refreshExpires = DateTime.UtcNow.AddDays(7); // 7 giorni per refresh token

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        var userProfile = new UserProfileDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Name = user.Name,
            Surname = user.Surname,
            Birthdate = user.Birthdate,
            Roles = roles.ToArray(),
            IsVerifiedArtist = user.IsVerifiedArtist,
            ArtistBio = user.ArtistBio,
            CreatedAt = user.CreatedAt,
        };

        var authResponse = new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            Expires = expires,
            User = userProfile,
        };

        return authResponse;
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string? ipAddress)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            Expires = DateTime.UtcNow.AddDays(7), // 7 giorni di validità
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            UserId = userId,
        };

        // Assicurati che il token sia unico
        var tokenExists = await context.RefreshTokens.AnyAsync(x => x.Token == refreshToken.Token);
        if (tokenExists)
            return await GenerateRefreshTokenAsync(userId, ipAddress); // Ricorsione per generarne uno nuovo

        return refreshToken;
    }

    private async Task<RefreshToken> GetRefreshTokenAsync(string token)
    {
        var refreshToken = await context
            .RefreshTokens.Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Token == token);

        return refreshToken?.IsActive == true
            ? refreshToken
            : throw new UnauthorizedAccessException("Invalid or expired refresh token");
    }

    private async Task RevokeRefreshTokenAsync(
        RefreshToken token,
        string? ipAddress,
        string? reason = null
    )
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReplacedByToken = reason;

        context.RefreshTokens.Update(token);
        await Task.CompletedTask;
    }

    private async Task RemoveOldRefreshTokensAsync(ApplicationUser user)
    {
        // Rimuovi refresh token scaduti e revocati più vecchi di 7 giorni
        var oldTokens = user
            .RefreshTokens.Where(x => !x.IsActive && x.Created.AddDays(7) <= DateTime.UtcNow)
            .ToList();

        if (oldTokens.Any())
        {
            context.RefreshTokens.RemoveRange(oldTokens);
            await Task.CompletedTask;
        }
    }
}
