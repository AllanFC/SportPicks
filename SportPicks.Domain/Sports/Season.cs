using System.Diagnostics.CodeAnalysis;

namespace Domain.Sports;

public class Season
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public required string DisplayName { get; set; }
    public Guid SportId { get; set; }
    public string? Type { get; set; } // Regular, Playoff, etc.
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Sport Sport { get; set; } = null!;
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    private Season() { } // Required for EF Core

    [SetsRequiredMembers]
    public Season(int year, string displayName, Guid sportId, DateTime startDate, DateTime endDate, bool isActive = false)
    {
        Id = Guid.NewGuid();
        Year = year;
        DisplayName = displayName;
        SportId = sportId;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSeason(string displayName, DateTime startDate, DateTime endDate, bool isActive, string? type = null)
    {
        DisplayName = displayName;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        Type = type;
        UpdatedAt = DateTime.UtcNow;
    }
}