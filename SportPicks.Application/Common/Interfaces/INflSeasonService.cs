namespace Application.Common.Interfaces;

/// <summary>
/// Service for determining NFL season information
/// </summary>
public interface INflSeasonService
{
    /// <summary>
    /// Gets the current NFL season year
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current NFL season year</returns>
    Task<int> GetCurrentSeasonAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the date range for a specific NFL season
    /// </summary>
    /// <param name="season">Season year</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of start and end dates for the season</returns>
    Task<(DateTime StartDate, DateTime EndDate)> GetSeasonDateRangeAsync(int season, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Determines if a given date falls within the NFL season
    /// </summary>
    /// <param name="date">Date to check</param>
    /// <param name="season">Season year</param>
    /// <returns>True if the date is within the season</returns>
    bool IsDateInSeason(DateTime date, int season);
}