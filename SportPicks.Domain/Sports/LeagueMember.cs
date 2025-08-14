using System.Diagnostics.CodeAnalysis;
using Domain.Users;

namespace Domain.Sports;

/// <summary>
/// Join table between User and League
/// </summary>
public class LeagueMember
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LeagueId { get; set; }
    public bool IsAdmin { get; set; } = false;
    public DateTime JoinedOn { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Scoring/ranking fields
    public int TotalPoints { get; set; } = 0;
    public int CorrectPicks { get; set; } = 0;
    public int TotalPicks { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual League League { get; set; } = null!;

    private LeagueMember() { } // Required for EF Core

    [SetsRequiredMembers]
    public LeagueMember(Guid userId, Guid leagueId, bool isAdmin = false)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        LeagueId = leagueId;
        IsAdmin = isAdmin;
        JoinedOn = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStats(int totalPoints, int correctPicks, int totalPicks)
    {
        TotalPoints = totalPoints;
        CorrectPicks = correctPicks;
        TotalPicks = totalPicks;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAdminStatus(bool isAdmin)
    {
        IsAdmin = isAdmin;
        UpdatedAt = DateTime.UtcNow;
    }
}