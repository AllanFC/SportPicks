using Application.Common.Interfaces;
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

    // NFL sport ID for consistency
    private static readonly Guid NflSportId = new("11111111-1111-1111-1111-111111111111");

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

            var season = new Season(espnSeason.Year, espnSeason.DisplayName, NflSportId, startDate, endDate, isActive);
            
            // Use AddOrUpdateAsync to handle existing seasons
            await _seasonRepository.AddOrUpdateAsync(season, cancellationToken);
            
            _logger.LogInformation("Successfully synced season {Year}: {DisplayName} ({StartDate} to {EndDate})",
                season.Year, season.DisplayName, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
            
            return season;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync season {Year}", year);
            return null;
        }
    }

    public async Task<Season?> SyncCurrentSeasonAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Syncing current NFL season from ESPN Core API");

        try
        {
            // Try current year first
            var currentYear = DateTime.UtcNow.Year;
            var season = await SyncSeasonAsync(currentYear, cancellationToken);
            
            if (season != null && season.IsActive)
            {
                _logger.LogInformation("Found active current season: {Year}", season.Year);
                return season;
            }

            // If current year season is not active, try previous year 
            // (NFL seasons span across calendar years)
            var previousYear = currentYear - 1;
            season = await SyncSeasonAsync(previousYear, cancellationToken);
            
            if (season != null && season.IsActive)
            {
                _logger.LogInformation("Found active season from previous year: {Year}", season.Year);
                return season;
            }

            // If neither worked, try next year (in case we're early in the year)
            var nextYear = currentYear + 1;
            season = await SyncSeasonAsync(nextYear, cancellationToken);
            
            if (season != null && season.IsActive)
            {
                _logger.LogInformation("Found active season for next year: {Year}", season.Year);
                return season;
            }

            _logger.LogWarning("No active NFL season found in current, previous, or next year");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync current NFL season");
            return null;
        }
    }

    public async Task<int> UpdateActiveSeasonStatusAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating active season status for all NFL seasons");

        try
        {
            var allSeasons = await _seasonRepository.GetBySportAsync(NflSportId, cancellationToken);
            var now = DateTime.UtcNow;
            var updatedCount = 0;

            foreach (var season in allSeasons)
            {
                var shouldBeActive = now >= season.StartDate && now <= season.EndDate;
                
                if (season.IsActive != shouldBeActive)
                {
                    season.UpdateSeason(season.DisplayName, season.StartDate, season.EndDate, shouldBeActive, season.Type);
                    await _seasonRepository.UpdateAsync(season, cancellationToken);
                    updatedCount++;
                    
                    _logger.LogInformation("Updated season {Year} active status to {IsActive}", 
                        season.Year, shouldBeActive);
                }
            }

            _logger.LogInformation("Updated active status for {Count} NFL seasons", updatedCount);
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update active season status");
            throw;
        }
    }
}