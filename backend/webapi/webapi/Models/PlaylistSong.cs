using System.ComponentModel.DataAnnotations;

namespace webapi.Models;

public class PlaylistSong
{
    public int PlaylistId { get; set; }
    public int SongId { get; set; }

    [Range(1, int.MaxValue)]
    public int Order { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public virtual Playlist Playlist { get; set; } = null!;
    public virtual Song Song { get; set; } = null!;
}
