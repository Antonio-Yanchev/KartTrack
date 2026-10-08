
using KartTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KartTrack.Api.Data;

public class KartTrackDbContext : DbContext
{
   public KartTrackDbContext(
      DbContextOptions<KartTrackDbContext> options)
      : base(options)
   {
   }

   public DbSet<Track> Tracks => Set<Track>();
   public DbSet<Session> Sessions => Set<Session>();

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      // Each session belongs to an existing track.
      modelBuilder.Entity<Session>()
         .HasOne<Track>()
         .WithMany()
         .HasForeignKey(s => s.TrackId)
         .OnDelete(DeleteBehavior.Restrict);

      // SQLite needs a conversion to order decimal lap times
      // directly in database queries.
      modelBuilder.Entity<Session>()
         .Property(s => s.FastestLap)
         .HasConversion<double>();

      modelBuilder.Entity<Session>()
         .Property(s => s.AverageLap)
         .HasConversion<double>();

      // Seed the two original tracks.
      modelBuilder.Entity<Track>().HasData(
         new Track
         {
               Id = 1,
               Name = "TeamSport Edmonton",
               Location = "London",
               IsIndoor = true
         },
         new Track
         {
               Id = 2,
               Name = "Daytona Milton Keynes",
               Location = "Milton Keynes",
               IsIndoor = false
         }
      );

      // Preserve the original demonstration sessions.
      modelBuilder.Entity<Session>().HasData(
         new Session
         {
               Id = 1,
               TrackId = 1,
               DriverName = "Antonio",
               Date = new DateTime(2026, 1, 1, 0, 0, 0,
                  DateTimeKind.Utc),
               FastestLap = 58.45m,
               AverageLap = 61.20m,
               TotalLaps = 12,
               KartNumber = 7,
               Position = 3,
               Notes = "First test session at TeamSport Edmonton"
         },
         new Session
         {
               Id = 2,
               TrackId = 2,
               DriverName = "Adam",
               Date = new DateTime(2026, 1, 1, 0, 0, 0,
                  DateTimeKind.Utc),
               FastestLap = 62.10m,
               AverageLap = 65.40m,
               TotalLaps = 10,
               KartNumber = 14,
               Position = 6,
               Notes = "Outdoor session at Daytona Milton Keynes"
         }
      );
   }
}
