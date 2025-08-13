namespace Application.Common.Interfaces;

/// <summary>
/// Repository interface for Team operations
/// </summary>
public interface ITeamRepository
{
    /// <summary>
    /// Gets a team by ESPN ID
    /// </summary>
    /// <param name="espnId">ESPN team ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Team if found, null otherwise</returns>
    Task<Team?> GetByEspnIdAsync(string espnId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all active teams
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active teams</returns>
    Task<List<Team>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new team
    /// </summary>
    /// <param name="team">Team to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(Team team, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing team
    /// </summary>
    /// <param name="team">Team to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(Team team, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds or updates multiple teams
    /// </summary>
    /// <param name="teams">Teams to add or update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddOrUpdateRangeAsync(IEnumerable<Team> teams, CancellationToken cancellationToken = default);
}