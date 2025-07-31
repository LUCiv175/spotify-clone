using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using webapi.Models;

namespace WebApi.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Song> Songs { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistSong> PlaylistSongs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configurazione ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Surname).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ArtistBio).HasMaxLength(1000);
            entity.Property(e => e.ArtistProfileImage).HasMaxLength(500);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();
        });

        // Configurazione Song
        builder.Entity<Song>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Genre).HasMaxLength(50);
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.CoverImagePath).HasMaxLength(500);

            entity
                .HasOne(s => s.Artist)
                .WithMany(u => u.Songs)
                .HasForeignKey(s => s.ArtistId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Genre);
        });

        // Configurazione Playlist
        builder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CoverImagePath).HasMaxLength(500);

            entity
                .HasOne(p => p.User)
                .WithMany(u => u.Playlists)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurazione PlaylistSong (Many-to-Many)
        builder.Entity<PlaylistSong>(entity =>
        {
            entity.HasKey(ps => new { ps.PlaylistId, ps.SongId });

            entity
                .HasOne(ps => ps.Playlist)
                .WithMany(p => p.PlaylistSongs)
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(ps => ps.Song)
                .WithMany(s => s.PlaylistSongs)
                .HasForeignKey(ps => ps.SongId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
