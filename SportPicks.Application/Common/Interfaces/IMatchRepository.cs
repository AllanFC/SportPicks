namespace Application.Common.Interfaces;

/// <summary>
/// Repository interface for Match operations
/// </summary>
public interface IMatchRepository
{
    /// <summary>
    /// Gets a match by ESPN ID
    /// </summary>
    /// <param name="espnId">ESPN match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Match if found, null otherwise</returns>
    Task<Match?> GetByEspnIdAsync(string espnId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets matches for a specific season
    /// </summary>
    /// <param name="season">Season year</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matches for the season</returns>
    Task<List<Match>> GetBySeasonAsync(int season, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets matches within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matches within the date range</returns>
    Task<List<Match>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new match
    /// </summary>
    /// <param name="match">Match to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(Match match, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing match
    /// </summary>
    /// <param name="match">Match to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(Match match, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds or updates multiple matches
    /// </summary>
    /// <param name="matches">Matches to add or update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddOrUpdateRangeAsync(IEnumerable<Match> matches, CancellationToken cancellationToken = default);
}