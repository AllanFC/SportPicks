using Domain.Sports;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Application database context following EF Core best practices
/// </summary>
public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // User entities
    public DbSet<User> Users => Set<User>();
    
    // Sports entities
    public DbSet<Sport> Sports => Set<Sport>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Competitor> Competitors => Set<Competitor>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventCompetitor> EventCompetitors => Set<EventCompetitor>();
    
    // League entities
    public DbSet<League> Leagues => Set<League>();
    public DbSet<LeagueMember> LeagueMembers => Set<LeagueMember>();
    
    // Pick entities
    public DbSet<Pick> Picks => Set<Pick>();
    public DbSet<RankedPick> RankedPicks => Set<RankedPick>();
    public DbSet<RankedPickDetail> RankedPickDetails => Set<RankedPickDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations
        ConfigureUserEntities(modelBuilder);
        ConfigureSportsEntities(modelBuilder);
        ConfigureLeagueEntities(modelBuilder);
        ConfigurePickEntities(modelBuilder);
        
        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureUserEntities(ModelBuilder modelBuilder)
    {
        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Required properties with constraints
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.PasswordHash)
                .IsRequired();
                
            entity.Property(e => e.UserRole)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.Provider)
                .IsRequired()
                .HasMaxLength(50);

            // Unique constraints
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email_Unique");

            // Performance indexes for common queries
            entity.HasIndex(e => e.UserRole)
                .HasDatabaseName("IX_Users_UserRole");
                
            entity.HasIndex(e => e.Provider)
                .HasDatabaseName("IX_Users_Provider");
        });
    }

    private static void ConfigureSportsEntities(ModelBuilder modelBuilder)
    {
        // Sport entity configuration
        modelBuilder.Entity<Sport>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(10);
                
            entity.Property(e => e.Description)
                .HasMaxLength(500);
                
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500);
            
            entity.HasIndex(e => e.Code)
                .IsUnique()
                .HasDatabaseName("IX_Sports_Code_Unique");
                
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Sports_IsActive");
        });

        // Season entity configuration
        modelBuilder.Entity<Season>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.Type)
                .HasMaxLength(50);

            // Foreign key relationship
            entity.HasOne(e => e.Sport)
                .WithMany(s => s.Seasons)
                .HasForeignKey(e => e.SportId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Performance indexes
            entity.HasIndex(e => e.SportId)
                .HasDatabaseName("IX_Seasons_SportId");
                
            entity.HasIndex(e => e.Year)
                .HasDatabaseName("IX_Seasons_Year");
                
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Seasons_IsActive");
                
            entity.HasIndex(e => new { e.SportId, e.Year })
                .HasDatabaseName("IX_Seasons_SportId_Year");
                
            entity.HasIndex(e => e.StartDate)
                .HasDatabaseName("IX_Seasons_StartDate");
                
            entity.HasIndex(e => e.EndDate)
                .HasDatabaseName("IX_Seasons_EndDate");
                
            entity.HasIndex(e => new { e.StartDate, e.EndDate })
                .HasDatabaseName("IX_Seasons_DateRange");
        });

        // Competitor entity configuration
        modelBuilder.Entity<Competitor>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(20);
                
            entity.Property(e => e.Location)
                .HasMaxLength(100);
                
            entity.Property(e => e.Nickname)
                .HasMaxLength(100);
                
            entity.Property(e => e.FirstName)
                .HasMaxLength(50);
                
            entity.Property(e => e.LastName)
                .HasMaxLength(50);
                
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500);
                
            entity.Property(e => e.Color)
                .HasMaxLength(20);
                
            entity.Property(e => e.AlternateColor)
                .HasMaxLength(20);
                
            entity.Property(e => e.ExternalId)
                .HasMaxLength(50);
                
            entity.Property(e => e.ExternalSource)
                .HasMaxLength(20);

            // Foreign key relationship
            entity.HasOne(e => e.Sport)
                .WithMany(s => s.Competitors)
                .HasForeignKey(e => e.SportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraints
            entity.HasIndex(e => new { e.SportId, e.Code })
                .IsUnique()
                .HasDatabaseName("IX_Competitors_SportId_Code_Unique");
                
            // Performance indexes
            entity.HasIndex(e => e.SportId)
                .HasDatabaseName("IX_Competitors_SportId");
                
            entity.HasIndex(e => e.Code)
                .HasDatabaseName("IX_Competitors_Code");
                
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Competitors_IsActive");
                
            entity.HasIndex(e => e.ExternalId)
                .HasDatabaseName("IX_Competitors_ExternalId");
                
            entity.HasIndex(e => new { e.ExternalSource, e.ExternalId })
                .HasDatabaseName("IX_Competitors_External");
        });

        // Event entity configuration
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
                
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.Venue)
                .HasMaxLength(200);
                
            entity.Property(e => e.Location)
                .HasMaxLength(200);
                
            entity.Property(e => e.EventType)
                .HasMaxLength(50);
                
            entity.Property(e => e.ExternalId)
                .HasMaxLength(50);
                
            entity.Property(e => e.ExternalSource)
                .HasMaxLength(20);

            // Foreign key relationship
            entity.HasOne(e => e.Season)
                .WithMany(s => s.Events)
                .HasForeignKey(e => e.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Performance indexes for common queries
            entity.HasIndex(e => e.SeasonId)
                .HasDatabaseName("IX_Events_SeasonId");
                
            entity.HasIndex(e => e.EventDate)
                .HasDatabaseName("IX_Events_EventDate");
                
            entity.HasIndex(e => e.IsCompleted)
                .HasDatabaseName("IX_Events_IsCompleted");
                
            entity.HasIndex(e => e.Week)
                .HasDatabaseName("IX_Events_Week");
                
            entity.HasIndex(e => e.Round)
                .HasDatabaseName("IX_Events_Round");
                
            entity.HasIndex(e => new { e.SeasonId, e.EventDate })
                .HasDatabaseName("IX_Events_SeasonId_EventDate");
                
            entity.HasIndex(e => new { e.SeasonId, e.Week })
                .HasDatabaseName("IX_Events_SeasonId_Week");
                
            entity.HasIndex(e => e.ExternalId)
                .HasDatabaseName("IX_Events_ExternalId");
                
            entity.HasIndex(e => new { e.ExternalSource, e.ExternalId })
                .HasDatabaseName("IX_Events_External");
        });

        // EventCompetitor entity configuration
        modelBuilder.Entity<EventCompetitor>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Status)
                .HasMaxLength(50);

            // Foreign key relationships
            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.EventCompetitors)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Competitor)
                .WithMany(c => c.EventCompetitors)
                .HasForeignKey(e => e.CompetitorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint - each competitor can only be in an event once
            entity.HasIndex(e => new { e.EventId, e.CompetitorId })
                .IsUnique()
                .HasDatabaseName("IX_EventCompetitors_EventId_CompetitorId_Unique");
                
            // Performance indexes
            entity.HasIndex(e => e.EventId)
                .HasDatabaseName("IX_EventCompetitors_EventId");
                
            entity.HasIndex(e => e.CompetitorId)
                .HasDatabaseName("IX_EventCompetitors_CompetitorId");
        });
    }

    private static void ConfigureLeagueEntities(ModelBuilder modelBuilder)
    {
        // League entity configuration
        modelBuilder.Entity<League>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
                
            entity.Property(e => e.InviteCode)
                .HasMaxLength(20);

            // Foreign key relationships
            entity.HasOne(e => e.Sport)
                .WithMany(s => s.Leagues)
                .HasForeignKey(e => e.SportId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraints
            entity.HasIndex(e => e.InviteCode)
                .IsUnique()
                .HasFilter($"[{nameof(League.InviteCode)}] IS NOT NULL")
                .HasDatabaseName("IX_Leagues_InviteCode_Unique");
                
            // Performance indexes
            entity.HasIndex(e => e.SportId)
                .HasDatabaseName("IX_Leagues_SportId");
                
            entity.HasIndex(e => e.CreatedByUserId)
                .HasDatabaseName("IX_Leagues_CreatedByUserId");
                
            entity.HasIndex(e => e.IsPublic)
                .HasDatabaseName("IX_Leagues_IsPublic");
                
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Leagues_IsActive");
        });

        // LeagueMember entity configuration
        modelBuilder.Entity<LeagueMember>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Foreign key relationships
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.League)
                .WithMany(l => l.LeagueMembers)
                .HasForeignKey(e => e.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint - user can only be in a league once
            entity.HasIndex(e => new { e.UserId, e.LeagueId })
                .IsUnique()
                .HasDatabaseName("IX_LeagueMembers_UserId_LeagueId_Unique");
                
            // Performance indexes
            entity.HasIndex(e => e.LeagueId)
                .HasDatabaseName("IX_LeagueMembers_LeagueId");
                
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_LeagueMembers_UserId");
                
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_LeagueMembers_IsActive");
                
            entity.HasIndex(e => new { e.LeagueId, e.TotalPoints })
                .HasDatabaseName("IX_LeagueMembers_LeagueId_TotalPoints");
        });
    }

    private static void ConfigurePickEntities(ModelBuilder modelBuilder)
    {
        // Pick entity configuration
        modelBuilder.Entity<Pick>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Notes)
                .HasMaxLength(1000);

            // Foreign key relationships
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.League)
                .WithMany(l => l.Picks)
                .HasForeignKey(e => e.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Picks)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PickedCompetitor)
                .WithMany(c => c.Picks)
                .HasForeignKey(e => e.PickedCompetitorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint - user can only pick once per event per league
            entity.HasIndex(e => new { e.UserId, e.LeagueId, e.EventId })
                .IsUnique()
                .HasDatabaseName("IX_Picks_UserId_LeagueId_EventId_Unique");
                
            // Performance indexes
            entity.HasIndex(e => e.LeagueId)
                .HasDatabaseName("IX_Picks_LeagueId");
                
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Picks_UserId");
                
            entity.HasIndex(e => e.EventId)
                .HasDatabaseName("IX_Picks_EventId");
                
            entity.HasIndex(e => e.PickedCompetitorId)
                .HasDatabaseName("IX_Picks_PickedCompetitorId");
                
            entity.HasIndex(e => new { e.LeagueId, e.EventId })
                .HasDatabaseName("IX_Picks_LeagueId_EventId");
        });

        // RankedPick entity configuration
        modelBuilder.Entity<RankedPick>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Notes)
                .HasMaxLength(1000);

            // Foreign key relationships
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.League)
                .WithMany(l => l.RankedPicks)
                .HasForeignKey(e => e.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.RankedPicks)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint - user can only make one ranked pick per event per league
            entity.HasIndex(e => new { e.UserId, e.LeagueId, e.EventId })
                .IsUnique()
                .HasDatabaseName("IX_RankedPicks_UserId_LeagueId_EventId_Unique");
                
            // Performance indexes
            entity.HasIndex(e => e.LeagueId)
                .HasDatabaseName("IX_RankedPicks_LeagueId");
                
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_RankedPicks_UserId");
                
            entity.HasIndex(e => e.EventId)
                .HasDatabaseName("IX_RankedPicks_EventId");
        });

        // RankedPickDetail entity configuration
        modelBuilder.Entity<RankedPickDetail>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Foreign key relationships
            entity.HasOne(e => e.RankedPick)
                .WithMany(rp => rp.RankedPickDetails)
                .HasForeignKey(e => e.RankedPickId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Competitor)
                .WithMany(c => c.RankedPickDetails)
                .HasForeignKey(e => e.CompetitorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint - each competitor can only be picked once per ranked pick
            entity.HasIndex(e => new { e.RankedPickId, e.CompetitorId })
                .IsUnique()
                .HasDatabaseName("IX_RankedPickDetails_RankedPickId_CompetitorId_Unique");
                
            // Performance indexes
            entity.HasIndex(e => e.RankedPickId)
                .HasDatabaseName("IX_RankedPickDetails_RankedPickId");
                
            entity.HasIndex(e => e.CompetitorId)
                .HasDatabaseName("IX_RankedPickDetails_CompetitorId");
                
            entity.HasIndex(e => new { e.RankedPickId, e.PredictedPosition })
                .HasDatabaseName("IX_RankedPickDetails_RankedPickId_PredictedPosition");
        });
    }
}
