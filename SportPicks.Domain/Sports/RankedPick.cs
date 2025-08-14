using System.Diagnostics.CodeAnalysis;
using Domain.Users;

namespace Domain.Sports;

/// <summary>
/// Container for ranked predictions (like F1 top-10)
/// </summary>
public class RankedPick
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LeagueId { get; set; }
    public Guid EventId { get; set; }
    
    // Scoring
    public int TotalPoints { get; set; } = 0;
    
    // Metadata
    public DateTime PickedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual League League { get; set; } = null!;
    public virtual Event Event { get; set; } = null!;
    public virtual ICollection<RankedPickDetail> RankedPickDetails { get; set; } = new List<RankedPickDetail>();

    private RankedPick() { } // Required for EF Core

    [SetsRequiredMembers]
    public RankedPick(Guid userId, Guid leagueId, Guid eventId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        LeagueId = leagueId;
        EventId = eventId;
        PickedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ScorePick(int totalPoints)
    {
        TotalPoints = totalPoints;
        UpdatedAt = DateTime.UtcNow;
    }
}