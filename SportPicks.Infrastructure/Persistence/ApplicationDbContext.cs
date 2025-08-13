using Domain.Sports;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Season> Seasons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.UserRole).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);

            // Add indexes for common queries
            entity.HasIndex(e => e.UserRole);
            entity.HasIndex(e => e.Provider);
        });

        // Team entity configuration
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.EspnId);
            entity.Property(e => e.EspnId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Abbreviation).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Nickname).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.AlternateColor).HasMaxLength(20);

            // Add indexes for common queries
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Abbreviation);
        });

        // Match entity configuration
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.EspnId);
            entity.Property(e => e.EspnId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SeasonTypeSlug).IsRequired().HasMaxLength(50);
            entity.Property(e => e.HomeTeamEspnId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AwayTeamEspnId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Venue).HasMaxLength(200);

            // Add indexes for common queries
            entity.HasIndex(e => e.Season);
            entity.HasIndex(e => e.Week);
            entity.HasIndex(e => e.MatchDate);
            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => new { e.Season, e.Week });
            entity.HasIndex(e => new { e.HomeTeamEspnId, e.AwayTeamEspnId, e.MatchDate });
        });

        // Season entity configuration
        modelBuilder.Entity<Season>(entity =>
        {
            entity.HasKey(e => e.Year);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(50);
            
            // Add indexes for common queries
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.EndDate);
            entity.HasIndex(e => new { e.StartDate, e.EndDate }); // For date range queries
        });
    }
}
