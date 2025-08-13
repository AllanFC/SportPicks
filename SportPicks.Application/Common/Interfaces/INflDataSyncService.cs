namespace Application.Common.Interfaces;

/// <summary>
/// Service for synchronizing NFL data from ESPN APIs
/// </summary>
public interface INflDataSyncService
{
    /// <summary>
    /// Synchronizes team data from ESPN API
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of teams synchronized</returns>
    Task<int> SyncTeamsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Synchronizes match schedule data from ESPN API for the current season
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of matches synchronized</returns>
    Task<int> SyncMatchesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Synchronizes match schedule data from ESPN API for a specific season
    /// </summary>
    /// <param name="season">Season year to synchronize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of matches synchronized</returns>
    Task<int> SyncMatchesForSeasonAsync(int season, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Synchronizes match schedule data from ESPN API for a specific date range
    /// </summary>
    /// <param name="startDate">Start date for synchronization</param>
    /// <param name="endDate">End date for synchronization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of matches synchronized</returns>
    Task<int> SyncMatchesAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Performs a full synchronization of both teams and matches
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple containing (teams synced, matches synced)</returns>
    Task<(int TeamsSynced, int MatchesSynced)> PerformFullSyncAsync(CancellationToken cancellationToken = default);
}