using System.Diagnostics.CodeAnalysis;
using Domain.Users;

namespace Domain.Sports;

/// <summary>
/// User's prediction for an Event in a specific League (for simple head-to-head picks)
/// </summary>
public class Pick
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LeagueId { get; set; }
    public Guid EventId { get; set; }
    public Guid PickedCompetitorId { get; set; }
    
    // Scoring
    public int Points { get; set; } = 0;
    public bool? IsCorrect { get; set; } // null = not yet determined
    
    // Metadata
    public DateTime PickedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual League League { get; set; } = null!;
    public virtual Event Event { get; set; } = null!;
    public virtual Competitor PickedCompetitor { get; set; } = null!;

    private Pick() { } // Required for EF Core

    [SetsRequiredMembers]
    public Pick(Guid userId, Guid leagueId, Guid eventId, Guid pickedCompetitorId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        LeagueId = leagueId;
        EventId = eventId;
        PickedCompetitorId = pickedCompetitorId;
        PickedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePick(Guid pickedCompetitorId, string? notes = null)
    {
        PickedCompetitorId = pickedCompetitorId;
        Notes = notes;
        PickedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ScorePick(int points, bool isCorrect)
    {
        Points = points;
        IsCorrect = isCorrect;
        UpdatedAt = DateTime.UtcNow;
    }
}