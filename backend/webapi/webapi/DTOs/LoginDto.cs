namespace webapi.DTOs;

using System.ComponentModel.DataAnnotations;

public class LoginDto
{
    [EmailAddress]
    public required string Email { get; set; }

    public required string Password { get; set; }
}
