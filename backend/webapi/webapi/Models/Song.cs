using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    [Required, MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? CoverImagePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsPublic { get; set; } = true;

    [Range(0, int.MaxValue)]
    public int PlayCount { get; set; } = 0;

    public long? FileSizeBytes { get; set; }

    [MaxLength(50)]
    public string? FileFormat { get; set; } // mp3, wav, flac, etc.

    [ForeignKey(nameof(ArtistId))]
    public virtual ApplicationUser Artist { get; set; } = null!;
    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
}
