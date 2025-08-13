namespace Application.Common.Interfaces;

/// <summary>
/// Interface for ESPN API client operations
/// </summary>
public interface IEspnApiClient
{
    /// <summary>
    /// Gets all NFL teams from ESPN API
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Raw JSON response from ESPN API</returns>
    Task<string?> GetTeamsJsonAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets NFL scoreboard data for a specific date range
    /// </summary>
    /// <param name="startDate">Start date for scoreboard data</param>
    /// <param name="endDate">End date for scoreboard data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Raw JSON response from ESPN API</returns>
    Task<string?> GetScoreboardJsonAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}