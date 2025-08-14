using Application.Common.Interfaces;
using Domain.Sports;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Competitor entity operations
/// </summary>
public sealed class CompetitorRepository : ICompetitorRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CompetitorRepository> _logger;

    public CompetitorRepository(ApplicationDbContext context, ILogger<CompetitorRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Competitor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Competitors
                .Include(c => c.Sport)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get competitor by ID: {Id}", id);
            throw;
        }
    }

    public async Task<Competitor?> GetByExternalIdAsync(string externalId, string externalSource = "ESPN", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalSource);

        try
        {
            return await _context.Competitors
                .Include(c => c.Sport)
                .FirstOrDefaultAsync(c => c.ExternalId == externalId && c.ExternalSource == externalSource, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get competitor by external ID: {ExternalId} from {ExternalSource}", externalId, externalSource);
            throw;
        }
    }

    public async Task<IEnumerable<Competitor>> GetBySportAsync(Guid sportId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Competitors
                .Where(c => c.SportId == sportId)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get competitors by sport: {SportId}", sportId);
            throw;
        }
    }

    public async Task<IEnumerable<Competitor>> GetActiveBySportAsync(Guid sportId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Competitors
                .Where(c => c.SportId == sportId && c.IsActive)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active competitors by sport: {SportId}", sportId);
            throw;
        }
    }

    public async Task<IEnumerable<Competitor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Competitors
                .Include(c => c.Sport)
                .AsNoTracking()
                .OrderBy(c => c.Sport.Name)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all competitors");
            throw;
        }
    }

    public async Task<Competitor> AddAsync(Competitor competitor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(competitor);

        try
        {
            _context.Competitors.Add(competitor);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Added competitor: {Name} ({Code}) for sport {SportId}", 
                competitor.Name, competitor.Code, competitor.SportId);
            
            return competitor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add competitor: {Name}", competitor.Name);
            throw;
        }
    }

    public async Task UpdateAsync(Competitor competitor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(competitor);

        try
        {
            _context.Competitors.Update(competitor);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Updated competitor: {Name} ({Code})", competitor.Name, competitor.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update competitor: {Name}", competitor.Name);
            throw;
        }
    }

    public async Task AddOrUpdateRangeAsync(IEnumerable<Competitor> competitors, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(competitors);
        
        var competitorsList = competitors.ToList();
        if (competitorsList.Count == 0)
        {
            _logger.LogWarning("No competitors provided for add/update operation");
            return;
        }

        _logger.LogInformation("Processing {Count} competitors for add/update", competitorsList.Count);

        try
        {
            foreach (var competitor in competitorsList)
            {
                if (string.IsNullOrWhiteSpace(competitor.ExternalId))
                {
                    // No external ID, just add as new
                    _context.Competitors.Add(competitor);
                    continue;
                }

                var existing = await GetByExternalIdAsync(competitor.ExternalId, competitor.ExternalSource ?? "ESPN", cancellationToken);
                
                if (existing != null)
                {
                    // Update existing competitor
                    existing.UpdateCompetitor(
                        competitor.Name,
                        competitor.Code,
                        competitor.Location,
                        competitor.Nickname,
                        competitor.FirstName,
                        competitor.LastName,
                        competitor.LogoUrl,
                        competitor.Color,
                        competitor.AlternateColor,
                        competitor.IsActive);
                    
                    _context.Competitors.Update(existing);
                }
                else
                {
                    // Add new competitor
                    _context.Competitors.Add(competitor);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully processed {Count} competitors", competitorsList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add/update competitors");
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var competitor = await _context.Competitors.FindAsync([id], cancellationToken);
            if (competitor != null)
            {
                _context.Competitors.Remove(competitor);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Deleted competitor: {Name} (ID: {Id})", competitor.Name, id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent competitor with ID: {Id}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete competitor with ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string externalId, string externalSource = "ESPN", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalSource);

        try
        {
            return await _context.Competitors
                .AsNoTracking()
                .AnyAsync(c => c.ExternalId == externalId && c.ExternalSource == externalSource, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check competitor existence: {ExternalId} from {ExternalSource}", externalId, externalSource);
            throw;
        }
    }
}