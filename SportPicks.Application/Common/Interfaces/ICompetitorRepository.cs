using Domain.Sports;

namespace Application.Common.Interfaces;

/// <summary>
/// Repository interface for Competitor entity operations
/// </summary>
public interface ICompetitorRepository
{
    /// <summary>
    /// Gets a competitor by their ID
    /// </summary>
    Task<Competitor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a competitor by their external ID and source
    /// </summary>
    Task<Competitor?> GetByExternalIdAsync(string externalId, string externalSource = "ESPN", CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets competitors by sport
    /// </summary>
    Task<IEnumerable<Competitor>> GetBySportAsync(Guid sportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active competitors by sport
    /// </summary>
    Task<IEnumerable<Competitor>> GetActiveBySportAsync(Guid sportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all competitors
    /// </summary>
    Task<IEnumerable<Competitor>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new competitor
    /// </summary>
    Task<Competitor> AddAsync(Competitor competitor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing competitor
    /// </summary>
    Task UpdateAsync(Competitor competitor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates multiple competitors
    /// </summary>
    Task AddOrUpdateRangeAsync(IEnumerable<Competitor> competitors, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a competitor
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a competitor exists by external ID
    /// </summary>
    Task<bool> ExistsAsync(string externalId, string externalSource = "ESPN", CancellationToken cancellationToken = default);
}