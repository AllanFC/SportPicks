using Domain.Sports;

namespace Application.Common.Interfaces;

/// <summary>
/// Repository interface for Event entity operations
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Gets an event by its ID
    /// </summary>
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an event by its external ID and source
    /// </summary>
    Task<Event?> GetByExternalIdAsync(string externalId, string externalSource = "ESPN", CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events by season
    /// </summary>
    Task<IEnumerable<Event>> GetBySeasonAsync(Guid seasonId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets upcoming events for a sport
    /// </summary>
    Task<IEnumerable<Event>> GetUpcomingBySportAsync(Guid sportId, int limit = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events by date range
    /// </summary>
    Task<IEnumerable<Event>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events for a specific week in a season
    /// </summary>
    Task<IEnumerable<Event>> GetBySeasonAndWeekAsync(Guid seasonId, int week, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all events
    /// </summary>
    Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new event
    /// </summary>
    Task<Event> AddAsync(Event eventEntity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing event
    /// </summary>
    Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates multiple events
    /// </summary>
    Task AddOrUpdateRangeAsync(IEnumerable<Event> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an event
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an event exists by external ID
    /// </summary>
    Task<bool> ExistsAsync(string externalId, string externalSource = "ESPN", CancellationToken cancellationToken = default);
}