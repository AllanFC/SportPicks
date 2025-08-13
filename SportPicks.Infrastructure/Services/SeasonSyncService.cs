using Domain.Sports;
using Infrastructure.ExternalApis.Espn.Dtos;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace Infrastructure.Services;

public class SeasonSyncService : ISeasonSyncService
{
    private readonly IEspnApiClient _espnApiClient;
    private readonly ISeasonRepository _seasonRepository;
    private readonly ILogger<SeasonSyncService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SeasonSyncService(
        IEspnApiClient espnApiClient,
        ISeasonRepository seasonRepository,
        ILogger<SeasonSyncService> logger)
    {
        _espnApiClient = espnApiClient;
        _seasonRepository = seasonRepository;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<Season?> SyncSeasonAsync(int year, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Syncing season {Year} from ESPN Core API", year);

        try
        {
            var seasonJson = await _espnApiClient.GetSeasonInfoJsonAsync(year, cancellationToken);
            
            if (string.IsNullOrEmpty(seasonJson))
            {
                _logger.LogWarning("No season data received from ESPN Core API for year {Year}", year);
                return null;
            }

            var espnSeason = JsonSerializer.Deserialize<EspnCoreSeasonResponse>(seasonJson, _jsonOptions);
            
            if (espnSeason == null)
            {
                _logger.LogWarning("Failed to deserialize season data for year {Year}", year);
                return null;
            }

            // Parse dates from ESPN API
            if (!DateTime.TryParse(espnSeason.StartDate, null, DateTimeStyles.RoundtripKind, out var startDate))
            {
                _logger.LogWarning("Invalid start date format for season {Year}: {StartDate}", year, espnSeason.StartDate);
                return null;
            }

            if (!DateTime.TryParse(espnSeason.EndDate, null, DateTimeStyles.RoundtripKind, out var endDate))
            {
                _logger.LogWarning("Invalid end date format for season {Year}: {EndDate}", year, espnSeason.EndDate);
                return null;
            }

            var now = DateTime.UtcNow;
            var isActive = now >= startDate && now <= endDate;

            var season = new Season(espnSeason.Year, espnSeason.DisplayName, startDate, endDate, isActive);
            
            await _seasonRepository.AddOrUpdateAsync(season, cancellationToken);

            _logger.LogInformation("Successfully synced season {Year}: {StartDate} to {EndDate} (Active: {IsActive})", 
                year, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), isActive);

            return season;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync season {Year} from ESPN Core API", year);
            throw;
        }
    }

    public async Task<Season?> SyncCurrentSeasonAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Syncing current season from ESPN Core API");

        try
        {
            var seasonJson = await _espnApiClient.GetCurrentSeasonInfoJsonAsync(cancellationToken);
            
            if (string.IsNullOrEmpty(seasonJson))
            {
                _logger.LogWarning("No current season data received from ESPN Core API");
                return null;
            }

            var espnSeason = JsonSerializer.Deserialize<EspnCoreSeasonResponse>(seasonJson, _jsonOptions);
            
            if (espnSeason == null)
            {
                _logger.LogWarning("Failed to deserialize current season data");
                return null;
            }

            if (!DateTime.TryParse(espnSeason.StartDate, null, DateTimeStyles.RoundtripKind, out var startDate))
            {
                _logger.LogWarning("Invalid start date format for current season: {StartDate}", espnSeason.StartDate);
                return null;
            }

            if (!DateTime.TryParse(espnSeason.EndDate, null, DateTimeStyles.RoundtripKind, out var endDate))
            {
                _logger.LogWarning("Invalid end date format for current season: {EndDate}", espnSeason.EndDate);
                return null;
            }

            var season = new Season(espnSeason.Year, espnSeason.DisplayName, startDate, endDate, true);
            
            await _seasonRepository.AddOrUpdateAsync(season, cancellationToken);

            _logger.LogInformation("Successfully synced current season {Year}: {StartDate} to {EndDate}", 
                espnSeason.Year, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            return season;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync current season from ESPN Core API");
            throw;
        }
    }

    public async Task<int> UpdateActiveSeasonStatusAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating active season status for all seasons");

        try
        {
            var allSeasons = await _seasonRepository.GetAllSeasonsAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var updatedCount = 0;

            foreach (var season in allSeasons)
            {
                var shouldBeActive = now >= season.StartDate && now <= season.EndDate;
                
                if (season.IsActive != shouldBeActive)
                {
                    season.UpdateSeason(season.DisplayName, season.StartDate, season.EndDate, shouldBeActive);
                    await _seasonRepository.UpdateAsync(season, cancellationToken);
                    updatedCount++;
                    
                    _logger.LogDebug("Updated season {Year} active status to {IsActive}", season.Year, shouldBeActive);
                }
            }

            _logger.LogInformation("Updated active status for {Count} seasons", updatedCount);
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update active season status");
            throw;
        }
    }
}