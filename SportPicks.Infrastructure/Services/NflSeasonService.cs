using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.Services;

/// <summary>
/// Service for determining NFL season information using ESPN API data
/// </summary>
public class NflSeasonService : INflSeasonService
{
    private readonly IEspnApiClient _espnApiClient;
    private readonly ILogger<NflSeasonService> _logger;
    private readonly NflSyncSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    // Cache for season information to avoid repeated API calls
    private int? _cachedCurrentSeason;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromHours(6); // Cache for 6 hours

    public NflSeasonService(
        IEspnApiClient espnApiClient,
        ILogger<NflSeasonService> logger,
        IOptions<NflSyncSettings> settings)
    {
        _espnApiClient = espnApiClient;
        _logger = logger;
        _settings = settings.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
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
        if (_cachedCurrentSeason.HasValue && DateTime.UtcNow < _cacheExpiry)
        {
            _logger.LogDebug("Using cached current season: {Season}", _cachedCurrentSeason.Value);
            return _cachedCurrentSeason.Value;
        }

        try
        {
            // Try to get current season from ESPN API by fetching recent scoreboard data
            _logger.LogDebug("Detecting current NFL season from ESPN API");
            
            var recentDate = DateTime.Now.AddDays(-7); // Look back 7 days
            var json = await _espnApiClient.GetScoreboardJsonAsync(recentDate, DateTime.Now, cancellationToken);
            
            if (!string.IsNullOrEmpty(json))
            {
                var response = JsonSerializer.Deserialize<Infrastructure.ExternalApis.Espn.Dtos.EspnScoreboardResponse>(json, _jsonOptions);
                if (response?.Season?.Year > 0)
                {
                    var detectedSeason = response.Season.Year;
                    _logger.LogInformation("Detected current NFL season from ESPN API: {Season}", detectedSeason);
                    
                    // Cache the result
                    _cachedCurrentSeason = detectedSeason;
                    _cacheExpiry = DateTime.Now.Add(_cacheTimeout);
                    
                    return detectedSeason;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to detect current season from ESPN API, falling back to date-based detection");
        }

        // Fallback to date-based calculation
        var fallbackSeason = _settings.CurrentSeason;
        _logger.LogInformation("Using date-based season detection: {Season}", fallbackSeason);
        
        // Cache the fallback result for shorter time
        _cachedCurrentSeason = fallbackSeason;
        _cacheExpiry = DateTime.UtcNow.Add(TimeSpan.FromHours(1));
        
        return fallbackSeason;
    }

    /// <inheritdoc />
    public async Task<(DateTime StartDate, DateTime EndDate)> GetSeasonDateRangeAsync(int season, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to get actual season dates by fetching a broad range and finding the actual boundaries
            _logger.LogDebug("Determining season date range for {Season}", season);
            
            // NFL seasons typically run from September to February of the following year
            var estimatedStart = new DateTime(season, 8, 1); // Start looking from August
            var estimatedEnd = new DateTime(season + 1, 3, 31); // End looking at March of next year
            
            var json = await _espnApiClient.GetScoreboardJsonAsync(estimatedStart, estimatedEnd, cancellationToken);
            
            if (!string.IsNullOrEmpty(json))
            {
                var response = JsonSerializer.Deserialize<Infrastructure.ExternalApis.Espn.Dtos.EspnScoreboardResponse>(json, _jsonOptions);
                if (response?.Events?.Any() == true)
                {
                    var events = response.Events.Where(e => !string.IsNullOrEmpty(e.Date));
                    if (events.Any())
                    {
                        var dates = events
                            .Select(e => DateTime.TryParse(e.Date, out var date) ? date : (DateTime?)null)
                            .Where(d => d.HasValue)
                            .Select(d => d!.Value)
                            .ToList();
                            
                        if (dates.Any())
                        {
                            var actualStart = dates.Min().Date;
                            var actualEnd = dates.Max().Date.AddDays(1); // Include the last day
                            
                            _logger.LogInformation("Detected actual season date range for {Season}: {Start} to {End}", 
                                season, actualStart.ToString("yyyy-MM-dd"), actualEnd.ToString("yyyy-MM-dd"));
                                
                            return (actualStart, actualEnd);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get actual season dates from ESPN API for season {Season}", season);
        }

        // Fallback to estimated dates
        var fallbackStart = new DateTime(season, 9, 1);
        var fallbackEnd = new DateTime(season + 1, 2, 28);
        
        _logger.LogInformation("Using estimated season date range for {Season}: {Start} to {End}", 
            season, fallbackStart.ToString("yyyy-MM-dd"), fallbackEnd.ToString("yyyy-MM-dd"));
            
        return (fallbackStart, fallbackEnd);
    }

    /// <inheritdoc />
    public bool IsDateInSeason(DateTime date, int season)
    {
        // NFL seasons span two calendar years
        // Season 2024 runs approximately September 2024 - February 2025
        var seasonStart = new DateTime(season, 7, 1); // Allow some buffer before preseason
        var seasonEnd = new DateTime(season + 1, 3, 31); // Allow buffer after Super Bowl
        
        var result = date >= seasonStart && date <= seasonEnd;
        
        _logger.LogDebug("Date {Date} is {InSeason} season {Season}", 
            date.ToString("yyyy-MM-dd"), result ? "in" : "not in", season);
            
        return result;
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