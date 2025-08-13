using System.ComponentModel.DataAnnotations;

namespace Application.NflSync.Dtos;

/// <summary>
/// Request DTO for date range match synchronization
/// </summary>
public class DateRangeSyncRequestDto
{
    /// <summary>
    /// Start date for synchronization (YYYY-MM-DD format)
    /// </summary>
    [Required(ErrorMessage = "Start date is required")]
    [DataType(DataType.Date)]
    public required string StartDate { get; set; }

    /// <summary>
    /// End date for synchronization (YYYY-MM-DD format)
    /// </summary>
    [Required(ErrorMessage = "End date is required")]
    [DataType(DataType.Date)]
    public required string EndDate { get; set; }

    /// <summary>
    /// Validates and parses the date range
    /// </summary>
    /// <returns>Tuple of parsed dates or validation errors</returns>
    public (bool IsValid, DateTime? Start, DateTime? End, string? ErrorMessage) ValidateAndParse()
    {
        if (!DateTime.TryParse(StartDate, out var start))
        {
            return (false, null, null, "Invalid start date format. Please use YYYY-MM-DD format.");
        }

        if (!DateTime.TryParse(EndDate, out var end))
        {
            return (false, null, null, "Invalid end date format. Please use YYYY-MM-DD format.");
        }

        if (start > end)
        {
            return (false, null, null, "Start date must be before or equal to end date.");
        }

        var maxRange = TimeSpan.FromDays(365); // 1 year max
        if (end - start > maxRange)
        {
            return (false, null, null, "Date range cannot exceed 365 days for performance reasons.");
        }

        return (true, start, end, null);
    }
}

/// <summary>
/// Request DTO for season-specific synchronization
/// </summary>
public class SeasonSyncRequestDto
{
    /// <summary>
    /// NFL season year (e.g., 2024)
    /// </summary>
    [Required(ErrorMessage = "Season is required")]
    [Range(1970, 2030, ErrorMessage = "Season must be between 1970 and 2030")]
    public int Season { get; set; }

    /// <summary>
    /// Validates the season year
    /// </summary>
    /// <returns>True if valid, error message if invalid</returns>
    public (bool IsValid, string? ErrorMessage) Validate()
    {
        var currentYear = DateTime.Now.Year;
        
        if (Season < 1970)
        {
            return (false, "Season cannot be earlier than 1970 (NFL-AFL merger).");
        }

        if (Season > currentYear + 2)
        {
            return (false, $"Season cannot be more than 2 years in the future (max: {currentYear + 2}).");
        }

        return (true, null);
    }
}