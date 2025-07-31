using System.ComponentModel.DataAnnotations;

namespace webapi.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(
        50,
        MinimumLength = 3,
        ErrorMessage = "Username must be between 3 and 50 characters"
    )]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number"
    )]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 50 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Surname is required")]
    [StringLength(
        50,
        MinimumLength = 1,
        ErrorMessage = "Surname must be between 1 and 50 characters"
    )]
    public string Surname { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? Birthdate { get; set; }

    public bool RegisterAsArtist { get; set; } = false;

    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
    public string? ArtistBio { get; set; }
}
