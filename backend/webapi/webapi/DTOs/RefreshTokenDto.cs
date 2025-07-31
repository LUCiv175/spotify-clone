namespace webapi.DTOs;

using System.ComponentModel.DataAnnotations;

public record RefreshTokenDto([Required] string RefreshToken);
