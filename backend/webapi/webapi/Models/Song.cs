using System.ComponentModel.DataAnnotations;

namespace webapi.Models;

public class Song
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string ArtistId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Genre { get; set; }

    public TimeSpan Duration { get; set; }

    [Required]
    public string FilePath { get; set; } = string.Empty;

    public string? CoverImagePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPublic { get; set; } = true;

    public int PlayCount { get; set; } = 0;

    // Navigazione
    public virtual ApplicationUser Artist { get; set; } = null!;
    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
}
