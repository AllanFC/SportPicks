using System.Diagnostics.CodeAnalysis;

namespace Domain.Sports;

/// <summary>
/// Represents a competitor - can be a team or individual (driver, player)
/// </summary>
public class Competitor
{
    public Guid Id { get; set; }
    public required string Name { get; set; } // Display name
    public required string Code { get; set; } // Short code/abbreviation
    public Guid SportId { get; set; }
    
    // For teams
    public string? Location { get; set; }
    public string? Nickname { get; set; }
    
    // For individuals
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    // Common properties
    public string? LogoUrl { get; set; }
    public string? Color { get; set; }
    public string? AlternateColor { get; set; }
    public bool IsActive { get; set; } = true;
    
    // External data source tracking (ESPN, API, etc.)
    public string? ExternalId { get; set; }
    public string? ExternalSource { get; set; } = "ESPN"; // Default to ESPN for NFL
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Sport Sport { get; set; } = null!;
    public virtual ICollection<EventCompetitor> EventCompetitors { get; set; } = new List<EventCompetitor>();
    public virtual ICollection<Pick> Picks { get; set; } = new List<Pick>();
    public virtual ICollection<RankedPickDetail> RankedPickDetails { get; set; } = new List<RankedPickDetail>();

    private Competitor() { } // Required for EF Core

    [SetsRequiredMembers]
    public Competitor(string name, string code, Guid sportId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Code = code;
        SportId = sportId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCompetitor(string name, string code, string? location = null, string? nickname = null, 
                               string? firstName = null, string? lastName = null, string? logoUrl = null, 
                               string? color = null, string? alternateColor = null, bool isActive = true)
    {
        Name = name;
        Code = code;
        Location = location;
        Nickname = nickname;
        FirstName = firstName;
        LastName = lastName;
        LogoUrl = logoUrl;
        Color = color;
        AlternateColor = alternateColor;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetExternalReference(string externalId, string externalSource = "ESPN")
    {
        ExternalId = externalId;
        ExternalSource = externalSource;
        UpdatedAt = DateTime.UtcNow;
    }
}