using Application.Common.Interfaces;
using Domain.Sports;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Season entity operations following EF Core best practices
/// </summary>
public sealed class SeasonRepository : ISeasonRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SeasonRepository> _logger;

    public SeasonRepository(ApplicationDbContext context, ILogger<SeasonRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Season?> GetByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Seasons
                .Include(s => s.Sport)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Year == year, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get season by year: {Year}", year);
            throw;
        }
    }

    public async Task<Season?> GetByYearAndSportAsync(int year, Guid sportId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Seasons
                .Include(s => s.Sport)
                .FirstOrDefaultAsync(s => s.Year == year && s.SportId == sportId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get season by year and sport: {Year}, {SportId}", year, sportId);
            throw;
        }
    }

    public async Task<Season?> GetCurrentActiveSeasonAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            return await _context.Seasons
                .Include(s => s.Sport)
                .AsNoTracking()
                .Where(s => s.StartDate <= now && s.EndDate >= now && s.IsActive)
                .OrderByDescending(s => s.Year)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current active season");
            throw;
        }
    }

    public async Task<Season?> GetActiveByYearAndSportAsync(int year, Guid sportId, CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            return await _context.Seasons
                .Include(s => s.Sport)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Year == year && s.SportId == sportId && 
                                         s.StartDate <= now && s.EndDate >= now && s.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active season by year and sport: {Year}, {SportId}", year, sportId);
            throw;
        }
    }

    public async Task<List<Season>> GetAllSeasonsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Seasons
                .Include(s => s.Sport)
                .AsNoTracking()
                .OrderBy(s => s.Sport.Name)
                .ThenByDescending(s => s.Year)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all seasons");
            throw;
        }
    }

    public async Task<List<Season>> GetBySportAsync(Guid sportId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Seasons
                .AsNoTracking()
                .Where(s => s.SportId == sportId)
                .OrderByDescending(s => s.Year)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get seasons by sport: {SportId}", sportId);
            throw;
        }
    }

    public async Task AddAsync(Season season, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(season);

        try
        {
            _context.Seasons.Add(season);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Added season: {Year} ({DisplayName}) for sport {SportId}", 
                season.Year, season.DisplayName, season.SportId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add season: {Year} for sport {SportId}", season.Year, season.SportId);
            throw;
        }
    }

    public async Task UpdateAsync(Season season, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(season);

        try
        {
            _context.Seasons.Update(season);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Updated season: {Year} ({DisplayName}) for sport {SportId}", 
                season.Year, season.DisplayName, season.SportId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update season: {Year} for sport {SportId}", season.Year, season.SportId);
            throw;
        }
    }

    public async Task AddOrUpdateAsync(Season season, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(season);

        try
        {
            var existingSeason = await GetByYearAndSportAsync(season.Year, season.SportId, cancellationToken);
            
            if (existingSeason == null)
            {
                await AddAsync(season, cancellationToken);
                _logger.LogInformation("Added new season: {Year} for sport {SportId} ({StartDate} to {EndDate})", 
                    season.Year, season.SportId, season.StartDate.ToString("yyyy-MM-dd"), season.EndDate.ToString("yyyy-MM-dd"));
            }
            else
            {
                existingSeason.UpdateSeason(season.DisplayName, season.StartDate, season.EndDate, season.IsActive, season.Type);
                await UpdateAsync(existingSeason, cancellationToken);
                _logger.LogInformation("Updated existing season: {Year} for sport {SportId} ({StartDate} to {EndDate})", 
                    season.Year, season.SportId, season.StartDate.ToString("yyyy-MM-dd"), season.EndDate.ToString("yyyy-MM-dd"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add or update season: {Year} for sport {SportId}", season.Year, season.SportId);
            throw;
        }
    }
}