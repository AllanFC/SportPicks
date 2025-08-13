using Domain.Sports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

/// <summary>
/// Service for determining NFL season information using database data with ESPN Core API fallback
/// </summary>
public class NflSeasonService : INflSeasonService
{
    private readonly ISeasonRepository _seasonRepository;
    private readonly ISeasonSyncService _seasonSyncService;
    private readonly ILogger<NflSeasonService> _logger;
    private readonly NflSyncSettings _settings;

    // Cache for current season to avoid repeated database calls
    private int? _cachedCurrentSeason;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(30); // Cache for 30 minutes

    public NflSeasonService(
        ISeasonRepository seasonRepository,
        ISeasonSyncService seasonSyncService,
        ILogger<NflSeasonService> logger,
        IOptions<NflSyncSettings> settings)
    {
        _seasonRepository = seasonRepository;
        _seasonSyncService = seasonSyncService;
        _logger = logger;
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public async Task<int> GetCurrentSeasonAsync(CancellationToken cancellationToken = default)
    {
        // Return target season if explicitly configured
        if (_settings.TargetSeason.HasValue)
        {
            _logger.LogDebug("Using configured target season: {Season}", _settings.TargetSeason.Value);
            return _settings.TargetSeason.Value;
        }

        // Check cache first
        if (_cachedCurrentSeason.HasValue && DateTime.Now < _cacheExpiry)
        {
            _logger.LogDebug("Using cached current season: {Season}", _cachedCurrentSeason.Value);
            return _cachedCurrentSeason.Value;
        }

        try
        {
            // Try to get current season from database first
            _logger.LogDebug("Getting current NFL season from database");
            
            var currentSeason = await _seasonRepository.GetCurrentActiveSeasonAsync(cancellationToken);
            
            if (currentSeason != null)
            {
                _logger.LogInformation("Found current active season in database: {Season} ({DisplayName})", 
                    currentSeason.Year, currentSeason.DisplayName);
                
                // Cache the result
                _cachedCurrentSeason = currentSeason.Year;
                _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                
                return currentSeason.Year;
            }

            // If no current season in database, try to sync from ESPN Core API
            _logger.LogInformation("No current season found in database, syncing from ESPN Core API");
            
            var syncedSeason = await _seasonSyncService.SyncCurrentSeasonAsync(cancellationToken);
            
            if (syncedSeason != null)
            {
                _logger.LogInformation("Successfully synced current season: {Season}", syncedSeason.Year);
                
                // Cache the result
                _cachedCurrentSeason = syncedSeason.Year;
                _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                
                return syncedSeason.Year;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get current season from database or ESPN Core API");
        }

        // Last resort: date-based fallback
        var fallbackSeason = DateTime.Now.Month >= 8 ? DateTime.Now.Year : DateTime.Now.Year - 1;
        _logger.LogWarning("Using date-based fallback for current season: {Season}", fallbackSeason);
        
        // Cache the fallback result for shorter time
        _cachedCurrentSeason = fallbackSeason;
        _cacheExpiry = DateTime.Now.Add(TimeSpan.FromMinutes(5));
        
        return fallbackSeason;
    }

    /// <inheritdoc />
    public async Task<(DateTime StartDate, DateTime EndDate)> GetSeasonDateRangeAsync(int season, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting season date range for {Season} from database", season);
            
            var seasonData = await _seasonRepository.GetByYearAsync(season, cancellationToken);
            
            if (seasonData != null)
            {
                _logger.LogInformation("Found season {Season} in database: {Start} to {End}", 
                    season, seasonData.StartDate.ToString("yyyy-MM-dd"), seasonData.EndDate.ToString("yyyy-MM-dd"));
                    
                return (seasonData.StartDate, seasonData.EndDate);
            }

            // If not in database, try to sync from ESPN Core API
            _logger.LogInformation("Season {Season} not found in database, syncing from ESPN Core API", season);
            
            var syncedSeason = await _seasonSyncService.SyncSeasonAsync(season, cancellationToken);
            
            if (syncedSeason != null)
            {
                _logger.LogInformation("Successfully synced season {Season}: {Start} to {End}", 
                    season, syncedSeason.StartDate.ToString("yyyy-MM-dd"), syncedSeason.EndDate.ToString("yyyy-MM-dd"));
                    
                return (syncedSeason.StartDate, syncedSeason.EndDate);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get season dates from database or ESPN Core API for season {Season}", season);
        }

        // Last resort fallback to estimated dates
        var fallbackStart = new DateTime(season, 8, 1);
        var fallbackEnd = new DateTime(season + 1, 2, 28);
        
        _logger.LogWarning("Using fallback estimated season date range for {Season}: {Start} to {End}", 
            season, fallbackStart.ToString("yyyy-MM-dd"), fallbackEnd.ToString("yyyy-MM-dd"));
            
        return (fallbackStart, fallbackEnd);
    }

    /// <inheritdoc />
    public bool IsDateInSeason(DateTime date, int season)
    {
        // Try to get accurate date range from database first
        try
        {
            var (startDate, endDate) = GetSeasonDateRangeAsync(season, CancellationToken.None).Result;
            var result = date >= startDate && date <= endDate;
            
            _logger.LogDebug("Date {Date} is {InSeason} season {Season} (database/ESPN Core API dates)", 
                date.ToString("yyyy-MM-dd"), result ? "in" : "not in", season);
                
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to use database/ESPN Core API for date check, using fallback logic");
        }

        // Fallback to estimated logic
        var seasonStart = new DateTime(season, 7, 1);
        var seasonEnd = new DateTime(season + 1, 3, 31);
        
        var fallbackResult = date >= seasonStart && date <= seasonEnd;
        
        _logger.LogDebug("Date {Date} is {InSeason} season {Season} (fallback logic)", 
            date.ToString("yyyy-MM-dd"), fallbackResult ? "in" : "not in", season);
            
        return fallbackResult;
    }

    /// <summary>
    /// Clears the cached season information (useful for testing or manual refresh)
    /// </summary>
    public void ClearCache()
    {
        _cachedCurrentSeason = null;
        _cacheExpiry = DateTime.MinValue;
        _logger.LogDebug("Cleared NFL season cache");
    }
}