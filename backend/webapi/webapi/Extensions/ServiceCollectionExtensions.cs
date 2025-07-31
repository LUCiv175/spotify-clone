using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApi.Data;
using webapi.Interfaces;
using webapi.Models;
using webapi.Services;

namespace webapi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string is required");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        // JWT Configuration
        services.Configure<JwtSettings>(opts =>
        {
            opts.SecretKey =
                Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? throw new InvalidOperationException("JWT_SECRET_KEY is required");
            opts.Issuer =
                Environment.GetEnvironmentVariable("JWT_ISSUER")
                ?? throw new InvalidOperationException("JWT_ISSUER is required");
            opts.Audience =
                Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                ?? throw new InvalidOperationException("JWT_AUDIENCE is required");
            opts.ExpiresInMinutes = int.Parse(
                Environment.GetEnvironmentVariable("JWT_EXPIRES_IN_MINUTES") ?? "60"
            );
            opts.RefreshExpiresInDays = int.Parse(
                Environment.GetEnvironmentVariable("JWT_REFRESH_EXPIRES_IN_DAYS") ?? "7"
            );
        });

        var jwtSecretKey =
            Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? throw new InvalidOperationException("JWT_SECRET_KEY is required");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var key = Encoding.UTF8.GetBytes(jwtSecretKey);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                    ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero,
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId =
                    Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
                    ?? throw new InvalidOperationException("GOOGLE_CLIENT_ID is required");
                options.ClientSecret =
                    Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
                    ?? throw new InvalidOperationException("GOOGLE_CLIENT_SECRET is required");
            });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireUser", policy => policy.RequireRole(AppRoles.User));
            options.AddPolicy("RequireArtist", policy => policy.RequireRole(AppRoles.Artist));
            options.AddPolicy("RequireAdmin", policy => policy.RequireRole(AppRoles.Admin));
            options.AddPolicy(
                "RequireVerifiedArtist",
                policy =>
                    policy.RequireRole(AppRoles.Artist).RequireClaim("is_verified_artist", "true")
            );
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "Spotify Clone API",
                    Version = "v1",
                    Description = "API for Spotify Clone application",
                }
            );

            c.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                }
            );

            c.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
                }
            );
        });

        return services;
    }
}
