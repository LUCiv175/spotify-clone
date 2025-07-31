using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi.Models;

public class Playlist
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? CoverImagePath { get; set; }

    public bool IsPublic { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();

    [NotMapped]
    public int SongCount => PlaylistSongs?.Count ?? 0;

    [NotMapped]
    public TimeSpan TotalDuration =>
        PlaylistSongs?.Sum(ps => ps.Song?.Duration.Ticks) is long ticks && ticks > 0
            ? new TimeSpan(ticks)
            : TimeSpan.Zero;
}
