using System.Diagnostics.CodeAnalysis;

namespace Domain.Sports;

/// <summary>
/// Represents a sport (NFL, F1, NBA, etc.)
/// </summary>
public class Sport
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; } // NFL, F1, NBA, etc.
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Season> Seasons { get; set; } = new List<Season>();
    public virtual ICollection<Competitor> Competitors { get; set; } = new List<Competitor>();
    public virtual ICollection<League> Leagues { get; set; } = new List<League>();

    private Sport() { } // Required for EF Core

    [SetsRequiredMembers]
    public Sport(string name, string code)
    {
        Id = Guid.NewGuid();
        Name = name;
        Code = code;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSport(string name, string code, string? description = null, string? logoUrl = null, bool isActive = true)
    {
        Name = name;
        Code = code;
        Description = description;
        LogoUrl = logoUrl;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}