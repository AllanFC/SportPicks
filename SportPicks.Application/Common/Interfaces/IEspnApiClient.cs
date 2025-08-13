namespace Application.Common.Interfaces;

/// <summary>
/// Interface for ESPN API client operations
/// </summary>
public interface IEspnApiClient
{
    /// <summary>
    /// Gets NFL teams data from ESPN API as JSON string
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Raw JSON response or null if no data</returns>
    Task<string?> GetTeamsJsonAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets NFL scoreboard data for a specific date range as JSON string
    /// </summary>
    /// <param name="startDate">Start date for scoreboard data</param>
    /// <param name="endDate">End date for scoreboard data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Raw JSON response or null if no data</returns>
    Task<string?> GetScoreboardJsonAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets NFL season information from ESPN Core API as JSON string
    /// </summary>
    /// <param name="season">Season year (e.g., 2025)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Season information JSON or null if unavailable</returns>
    Task<string?> GetSeasonInfoJsonAsync(int season, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current NFL season information from ESPN Core API as JSON string
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current season information JSON or null if unavailable</returns>
    Task<string?> GetCurrentSeasonInfoJsonAsync(CancellationToken cancellationToken = default);
}