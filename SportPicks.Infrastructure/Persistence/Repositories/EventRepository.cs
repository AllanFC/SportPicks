using Application.Common.Interfaces;
using Domain.Sports;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Event entity operations
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(ApplicationDbContext context, ILogger<EventRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Season)
                .ThenInclude(s => s.Sport)
            .Include(e => e.EventCompetitors)
                .ThenInclude(ec => ec.Competitor)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Event?> GetByExternalIdAsync(string externalId, string externalSource = "ESPN", CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Season)
                .ThenInclude(s => s.Sport)
            .Include(e => e.EventCompetitors)
                .ThenInclude(ec => ec.Competitor)
            .FirstOrDefaultAsync(e => e.ExternalId == externalId && e.ExternalSource == externalSource, cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetBySeasonAsync(Guid seasonId, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.EventCompetitors)
                .ThenInclude(ec => ec.Competitor)
            .Where(e => e.SeasonId == seasonId)
            .OrderBy(e => e.EventDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetUpcomingBySportAsync(Guid sportId, int limit = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Season)
                .ThenInclude(s => s.Sport)
            .Include(e => e.EventCompetitors)
                .ThenInclude(ec => ec.Competitor)
            .Where(e => e.Season.SportId == sportId && 
                       e.EventDate > DateTime.UtcNow && 
                       !e.IsCompleted)
            .OrderBy(e => e.EventDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Season)
                .ThenInclude(s => s.Sport)
            .Include(e => e.EventCompetitors)
                .ThenInclude(ec => ec.Competitor)
            .Where(e => e.EventDate >= startDate && e.EventDate <= endDate)
            .OrderBy(e => e.EventDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetBySeasonAndWeekAsync(Guid seasonId, int week, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.EventCompetitors)
                .ThenInclude(ec => ec.Competitor)
            .Where(e => e.SeasonId == seasonId && e.Week == week)
            .OrderBy(e => e.EventDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Season)
                .ThenInclude(s => s.Sport)
            .OrderByDescending(e => e.EventDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Event> AddAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync(cancellationToken);
        return eventEntity;
    }

    public async Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        _context.Events.Update(eventEntity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddOrUpdateRangeAsync(IEnumerable<Event> events, CancellationToken cancellationToken = default)
    {
        var eventsList = events.ToList();
        _logger.LogInformation("Processing {Count} events for add/update", eventsList.Count);

        foreach (var eventEntity in eventsList)
        {
            var existing = string.IsNullOrEmpty(eventEntity.ExternalId) 
                ? null 
                : await GetByExternalIdAsync(eventEntity.ExternalId, eventEntity.ExternalSource ?? "ESPN", cancellationToken);

            if (existing != null)
            {
                // Update existing event
                existing.UpdateEvent(
                    eventEntity.Name,
                    eventEntity.EventDate,
                    eventEntity.Status,
                    eventEntity.IsCompleted,
                    eventEntity.Venue,
                    eventEntity.Location,
                    eventEntity.Week,
                    eventEntity.Round,
                    eventEntity.EventType);

                // CRITICAL FIX: Update EventCompetitors with new scores and winner information
                await UpdateEventCompetitorsAsync(existing, eventEntity.EventCompetitors, cancellationToken);

                _context.Events.Update(existing);
            }
            else
            {
                // Add new event
                _context.Events.Add(eventEntity);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully processed {Count} events", eventsList.Count);
    }

    /// <summary>
    /// Updates EventCompetitors for an existing event with new scores and winner information
    /// This is critical for properly updating game results when syncing from ESPN
    /// </summary>
    private async Task UpdateEventCompetitorsAsync(Event existingEvent, ICollection<EventCompetitor> newEventCompetitors, CancellationToken cancellationToken)
    {
        foreach (var newEventCompetitor in newEventCompetitors)
        {
            // Find matching existing EventCompetitor by CompetitorId and IsHomeTeam
            var existingEventCompetitor = existingEvent.EventCompetitors
                .FirstOrDefault(ec => ec.CompetitorId == newEventCompetitor.CompetitorId);

            if (existingEventCompetitor != null)
            {
                // Update the existing EventCompetitor with new results
                existingEventCompetitor.UpdateResult(
                    score: newEventCompetitor.Score,
                    position: newEventCompetitor.Position,
                    isWinner: newEventCompetitor.IsWinner,
                    status: newEventCompetitor.Status,
                    time: newEventCompetitor.Time);

                _logger.LogDebug("Updated EventCompetitor for Event {EventId}, Competitor {CompetitorId}: Score={Score}, IsWinner={IsWinner}",
                    existingEvent.ExternalId, existingEventCompetitor.CompetitorId, 
                    newEventCompetitor.Score, newEventCompetitor.IsWinner);
            }
            else
            {
                // This shouldn't happen normally, but add the EventCompetitor if it doesn't exist
                existingEvent.EventCompetitors.Add(newEventCompetitor);
                _logger.LogWarning("Added missing EventCompetitor for Event {EventId}, Competitor {CompetitorId}",
                    existingEvent.ExternalId, newEventCompetitor.CompetitorId);
            }
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _context.Events.FindAsync(new object[] { id }, cancellationToken);
        if (eventEntity != null)
        {
            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string externalId, string externalSource = "ESPN", CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AnyAsync(e => e.ExternalId == externalId && e.ExternalSource == externalSource, cancellationToken);
    }
}