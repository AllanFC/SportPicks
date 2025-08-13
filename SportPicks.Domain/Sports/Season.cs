using System.Diagnostics.CodeAnalysis;

namespace Domain.Sports;

public class Season
{
    public int Year { get; set; }
    public required string DisplayName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private Season() { } // Required for EF Core

    [SetsRequiredMembers]
    public Season(int year, string displayName, DateTime startDate, DateTime endDate, bool isActive = false)
    {
        Year = year;
        DisplayName = displayName;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSeason(string displayName, DateTime startDate, DateTime endDate, bool isActive)
    {
        DisplayName = displayName;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}