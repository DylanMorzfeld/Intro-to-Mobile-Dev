using FitTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Api.Data;

public class WorkoutDbContext : DbContext
{
    public WorkoutDbContext(DbContextOptions<WorkoutDbContext> options) : base(options)
    {
    }

    public DbSet<Workout> Workouts => Set<Workout>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workout>(entity =>
        {
            entity.ToTable("Workouts");
            entity.HasKey(w => w.Id);

            // Enums are stored as readable text ("Running"), matching the JSON the API sends.
            entity.Property(w => w.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(w => w.Intensity).HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.Property(w => w.Notes).HasMaxLength(500);
            entity.Property(w => w.CreatedUtc).IsRequired();

            // The database enforces the business rule too: one workout per type per day.
            // This is the backstop if two requests ever pass the service check at the same moment.
            entity.HasIndex(w => new { w.Date, w.Type }).IsUnique();
        });
    }
}
