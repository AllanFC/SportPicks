using System.Diagnostics.CodeAnalysis;

namespace Domain.Sports;

/// <summary>
/// Join table linking an Event to its participants (competitors)
/// </summary>
public class EventCompetitor
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid CompetitorId { get; set; }
    
    // For team sports (home/away)
    public bool? IsHomeTeam { get; set; }
    
    // Scores/results
    public int? Score { get; set; }
    public int? Position { get; set; } // Final position (1st, 2nd, etc.)
    public bool? IsWinner { get; set; }
    
    // Additional metadata
    public string? Status { get; set; } // Playing, Injured, DNF, etc.
    public TimeSpan? Time { get; set; } // For racing sports
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Event Event { get; set; } = null!;
    public virtual Competitor Competitor { get; set; } = null!;

    private EventCompetitor() { } // Required for EF Core

    [SetsRequiredMembers]
    public EventCompetitor(Guid eventId, Guid competitorId)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        CompetitorId = competitorId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateResult(int? score = null, int? position = null, bool? isWinner = null, 
                           string? status = null, TimeSpan? time = null)
    {
        Score = score;
        Position = position;
        IsWinner = isWinner;
        Status = status;
        Time = time;
        UpdatedAt = DateTime.UtcNow;
    }
}