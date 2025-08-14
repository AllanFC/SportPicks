using System.Diagnostics.CodeAnalysis;
using Domain.Users;

namespace Domain.Sports;

/// <summary>
/// Represents a user-created or official pick'em league
/// </summary>
public class League
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid SportId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public bool IsPublic { get; set; }
    public bool IsOfficial { get; set; } // System-created leagues
    public string? InviteCode { get; set; } // For private leagues
    public int MaxMembers { get; set; } = 100;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Sport Sport { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual ICollection<LeagueMember> LeagueMembers { get; set; } = new List<LeagueMember>();
    public virtual ICollection<Pick> Picks { get; set; } = new List<Pick>();
    public virtual ICollection<RankedPick> RankedPicks { get; set; } = new List<RankedPick>();

    private League() { } // Required for EF Core

    [SetsRequiredMembers]
    public League(string name, Guid sportId, Guid createdByUserId, bool isPublic = true)
    {
        Id = Guid.NewGuid();
        Name = name;
        SportId = sportId;
        CreatedByUserId = createdByUserId;
        IsPublic = isPublic;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        if (!isPublic)
        {
            InviteCode = GenerateInviteCode();
        }
    }

    public void UpdateLeague(string name, string? description = null, bool? isPublic = null, 
                           int? maxMembers = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        Name = name;
        Description = description;
        if (isPublic.HasValue) IsPublic = isPublic.Value;
        if (maxMembers.HasValue) MaxMembers = maxMembers.Value;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
        
        if (!IsPublic && string.IsNullOrEmpty(InviteCode))
        {
            InviteCode = GenerateInviteCode();
        }
        else if (IsPublic)
        {
            InviteCode = null;
        }
    }

    private static string GenerateInviteCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpper();
    }
}