using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace webapi.Models;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Surname { get; set; } = string.Empty;

    public DateTime? Birthdate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Proprietà per artisti
    public string? ArtistBio { get; set; }
    public string? ArtistProfileImage { get; set; }
    public bool IsVerifiedArtist { get; set; } = false;

    // Navigazione
    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
}
