namespace Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Team> Teams { get; set; } = null!;
    public DbSet<Match> Matches { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(u => u.Email)
                 .IsUnique();

            entity.Property(u => u.PasswordHash)
                .IsRequired();

            entity.Property(u => u.Salt)
                .IsRequired();

            entity.Property(u => u.Provider)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(u => u.CreatedAt)
                .IsRequired();
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(t => t.EspnId);
            
            entity.Property(t => t.EspnId)
                .HasMaxLength(50);

            entity.Property(t => t.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.Abbreviation)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(t => t.Location)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.Nickname)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.LogoUrl)
                .HasMaxLength(500);

            entity.Property(t => t.Color)
                .HasMaxLength(20);

            entity.Property(t => t.AlternateColor)
                .HasMaxLength(20);

            entity.HasIndex(t => t.Abbreviation);
            entity.HasIndex(t => t.IsActive);
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(m => m.EspnId);
            
            entity.Property(m => m.EspnId)
                .HasMaxLength(50);

            entity.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(m => m.HomeTeamEspnId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(m => m.AwayTeamEspnId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(m => m.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(m => m.Venue)
                .HasMaxLength(200);

            entity.HasIndex(m => m.MatchDate);
            entity.HasIndex(m => m.Season);
            entity.HasIndex(m => m.Week);
            entity.HasIndex(m => m.IsCompleted);
            entity.HasIndex(m => new { m.Season, m.Week });
            entity.HasIndex(m => new { m.HomeTeamEspnId, m.AwayTeamEspnId, m.MatchDate });
        });
    }
}
