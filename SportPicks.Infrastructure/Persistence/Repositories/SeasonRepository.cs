using Domain.Sports;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Repositories;

public class SeasonRepository : ISeasonRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SeasonRepository> _logger;

    public SeasonRepository(ApplicationDbContext context, ILogger<SeasonRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Season?> GetByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Seasons
                .FirstOrDefaultAsync(s => s.Year == year, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get season by year: {Year}", year);
            throw;
        }
    }

    public async Task<Season?> GetCurrentActiveSeasonAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            return await _context.Seasons
                .Where(s => s.StartDate <= now && s.EndDate >= now)
                .OrderByDescending(s => s.Year)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current active season");
            throw;
        }
    }

    public async Task<List<Season>> GetAllSeasonsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Seasons
                .OrderBy(s => s.Year)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all seasons");
            throw;
        }
    }

    public async Task AddAsync(Season season, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Seasons.Add(season);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Added season: {Year} ({DisplayName})", season.Year, season.DisplayName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add season: {Year}", season.Year);
            throw;
        }
    }

    public async Task UpdateAsync(Season season, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Seasons.Update(season);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Updated season: {Year} ({DisplayName})", season.Year, season.DisplayName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update season: {Year}", season.Year);
            throw;
        }
    }

    public async Task AddOrUpdateAsync(Season season, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingSeason = await GetByYearAsync(season.Year, cancellationToken);
            
            if (existingSeason == null)
            {
                await AddAsync(season, cancellationToken);
                _logger.LogInformation("Added new season: {Year} ({StartDate} to {EndDate})", 
                    season.Year, season.StartDate.ToString("yyyy-MM-dd"), season.EndDate.ToString("yyyy-MM-dd"));
            }
            else
            {
                existingSeason.UpdateSeason(season.DisplayName, season.StartDate, season.EndDate, season.IsActive);
                await UpdateAsync(existingSeason, cancellationToken);
                _logger.LogInformation("Updated existing season: {Year} ({StartDate} to {EndDate})", 
                    season.Year, season.StartDate.ToString("yyyy-MM-dd"), season.EndDate.ToString("yyyy-MM-dd"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add or update season: {Year}", season.Year);
            throw;
        }
    }
}