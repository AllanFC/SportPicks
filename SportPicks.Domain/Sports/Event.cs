using System.Diagnostics.CodeAnalysis;

namespace Domain.Sports;

/// <summary>
/// Represents an event (match, race, game) in a season
/// </summary>
public class Event
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid SeasonId { get; set; }
    public DateTime EventDate { get; set; }
    public required string Status { get; set; } // Scheduled, Live, Completed, Cancelled, etc.
    public bool IsCompleted { get; set; }
    
    // Optional properties
    public string? Venue { get; set; }
    public string? Location { get; set; }
    public int? Week { get; set; } // For sports with weekly structure
    public int? Round { get; set; } // For sports with round structure
    public string? EventType { get; set; } // Regular, Playoff, Championship, etc.
    
    // External data source tracking (ESPN, API, etc.)
    public string? ExternalId { get; set; }
    public string? ExternalSource { get; set; } = "ESPN"; // Default to ESPN for NFL
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Season Season { get; set; } = null!;
    public virtual ICollection<EventCompetitor> EventCompetitors { get; set; } = new List<EventCompetitor>();
    public virtual ICollection<Pick> Picks { get; set; } = new List<Pick>();
    public virtual ICollection<RankedPick> RankedPicks { get; set; } = new List<RankedPick>();

    private Event() { } // Required for EF Core

    [SetsRequiredMembers]
    public Event(string name, Guid seasonId, DateTime eventDate, string status)
    {
        Id = Guid.NewGuid();
        Name = name;
        SeasonId = seasonId;
        EventDate = eventDate;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEvent(string name, DateTime eventDate, string status, bool isCompleted, 
                           string? venue = null, string? location = null, int? week = null, 
                           int? round = null, string? eventType = null)
    {
        Name = name;
        EventDate = eventDate;
        Status = status;
        IsCompleted = isCompleted;
        Venue = venue;
        Location = location;
        Week = week;
        Round = round;
        EventType = eventType;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetExternalReference(string externalId, string externalSource = "ESPN")
    {
        ExternalId = externalId;
        ExternalSource = externalSource;
        UpdatedAt = DateTime.UtcNow;
    }
}