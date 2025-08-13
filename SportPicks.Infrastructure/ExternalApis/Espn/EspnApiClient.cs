using Infrastructure.ExternalApis.Espn.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.ExternalApis.Espn;

/// <summary>
/// HTTP client for communicating with ESPN NFL API
/// </summary>
public class EspnApiClient : IEspnApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EspnApiClient> _logger;
    private readonly NflSyncSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public EspnApiClient(HttpClient httpClient, ILogger<EspnApiClient> logger, IOptions<NflSyncSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    /// <inheritdoc />
    public async Task<string?> GetTeamsJsonAsync(CancellationToken cancellationToken = default)
    {
        const string endpoint = "/apis/site/v2/sports/football/nfl/teams";
        _logger.LogInformation("Fetching teams data from ESPN API: {Endpoint}", endpoint);

        try
        {
            var response = await ExecuteWithRetryAsync(endpoint, cancellationToken);
            
            if (response != null)
            {
                _logger.LogInformation("Successfully fetched teams JSON from ESPN API");
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch teams from ESPN API");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetScoreboardJsonAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var startDateStr = startDate.ToString("yyyyMMdd");
        var endDateStr = endDate.ToString("yyyyMMdd");
        var endpoint = $"/apis/site/v2/sports/football/nfl/scoreboard?limit={_settings.ScoreboardLimit}&dates={startDateStr}-{endDateStr}";
        
        _logger.LogInformation("Fetching scoreboard data from ESPN API: {Endpoint}", endpoint);
        _logger.LogDebug("Date range: {StartDate} to {EndDate}", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

        try
        {
            var response = await ExecuteWithRetryAsync(endpoint, cancellationToken);
            
            if (response != null)
            {
                _logger.LogInformation("Successfully fetched scoreboard JSON from ESPN API for date range {StartDate} to {EndDate}", 
                    startDateStr, endDateStr);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch scoreboard from ESPN API for date range {StartDate} to {EndDate}", 
                startDateStr, endDateStr);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetSeasonInfoJsonAsync(int season, CancellationToken cancellationToken = default)
    {
        // Use ESPN Core API for season information
        var coreApiUrl = $"https://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{season}";
        
        _logger.LogInformation("Fetching season info from ESPN Core API: {Season}", season);

        try
        {
            // Create a separate HttpClient request for Core API since it's a different base URL
            using var coreResponse = await _httpClient.GetAsync(coreApiUrl, cancellationToken);
            
            if (coreResponse.IsSuccessStatusCode)
            {
                var content = await coreResponse.Content.ReadAsStringAsync(cancellationToken);
                
                _logger.LogInformation("Successfully fetched season {Season} info from ESPN Core API", season);
                return content;
            }
            else
            {
                _logger.LogWarning("ESPN Core API returned {StatusCode} for season {Season}", coreResponse.StatusCode, season);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch season {Season} info from ESPN Core API", season);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetCurrentSeasonInfoJsonAsync(CancellationToken cancellationToken = default)
    {
        // Try to get current season by checking the most likely current year first
        var currentYear = DateTime.Now.Year;
        
        // NFL season logic: if we're in Jan-July, try previous year first, otherwise current year
        var primaryYear = DateTime.Now.Month >= 8 ? currentYear : currentYear - 1;
        var secondaryYear = DateTime.Now.Month >= 8 ? currentYear + 1 : currentYear;

        _logger.LogDebug("Attempting to get current NFL season info (trying {PrimaryYear} first, then {SecondaryYear})", 
            primaryYear, secondaryYear);

        // Try primary year first
        var seasonJson = await GetSeasonInfoJsonAsync(primaryYear, cancellationToken);
        if (seasonJson != null)
        {
            // Check if this season is currently active based on dates
            if (IsSeasonCurrentlyActive(seasonJson))
            {
                _logger.LogInformation("Found current active season: {Season}", primaryYear);
                return seasonJson;
            }
        }

        // Try secondary year
        seasonJson = await GetSeasonInfoJsonAsync(secondaryYear, cancellationToken);
        if (seasonJson != null && IsSeasonCurrentlyActive(seasonJson))
        {
            _logger.LogInformation("Found current active season: {Season}", secondaryYear);
            return seasonJson;
        }

        _logger.LogWarning("Could not determine current active NFL season, falling back to primary year {Year}", primaryYear);
        return await GetSeasonInfoJsonAsync(primaryYear, cancellationToken);
    }

    /// <summary>
    /// Gets all NFL teams from ESPN API (with typed response)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Teams response from ESPN API</returns>
    public async Task<EspnTeamsResponse?> GetTeamsAsync(CancellationToken cancellationToken = default)
    {
        var json = await GetTeamsJsonAsync(cancellationToken);
        if (string.IsNullOrEmpty(json))
            return null;

        var teamsResponse = JsonSerializer.Deserialize<EspnTeamsResponse>(json, _jsonOptions);
        var teamCount = teamsResponse?.Sports.FirstOrDefault()?.Leagues.FirstOrDefault()?.Teams.Count ?? 0;
        _logger.LogInformation("Successfully parsed {TeamCount} teams from ESPN API", teamCount);
        
        return teamsResponse;
    }

    /// <summary>
    /// Gets NFL scoreboard data for a specific date range (with typed response)
    /// </summary>
    /// <param name="startDate">Start date for scoreboard data</param>
    /// <param name="endDate">End date for scoreboard data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scoreboard response from ESPN API</returns>
    public async Task<EspnScoreboardResponse?> GetScoreboardAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var json = await GetScoreboardJsonAsync(startDate, endDate, cancellationToken);
        if (string.IsNullOrEmpty(json))
            return null;

        var scoreboardResponse = JsonSerializer.Deserialize<EspnScoreboardResponse>(json, _jsonOptions);
        var eventCount = scoreboardResponse?.Events.Count ?? 0;
        _logger.LogInformation("Successfully parsed {EventCount} events from ESPN API", eventCount);
        
        return scoreboardResponse;
    }

    /// <summary>
    /// Gets NFL season information from ESPN Core API (with typed response)
    /// </summary>
    /// <param name="season">Season year</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Season information</returns>
    public async Task<EspnCoreSeasonResponse?> GetSeasonInfoAsync(int season, CancellationToken cancellationToken = default)
    {
        var json = await GetSeasonInfoJsonAsync(season, cancellationToken);
        if (string.IsNullOrEmpty(json))
            return null;

        var seasonResponse = JsonSerializer.Deserialize<EspnCoreSeasonResponse>(json, _jsonOptions);
        _logger.LogDebug("Successfully parsed season {Season} info from ESPN Core API", season);
        
        return seasonResponse;
    }

    /// <summary>
    /// Gets current NFL season information from ESPN Core API (with typed response)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current season information</returns>
    public async Task<EspnCoreSeasonResponse?> GetCurrentSeasonInfoAsync(CancellationToken cancellationToken = default)
    {
        var json = await GetCurrentSeasonInfoJsonAsync(cancellationToken);
        if (string.IsNullOrEmpty(json))
            return null;

        var seasonResponse = JsonSerializer.Deserialize<EspnCoreSeasonResponse>(json, _jsonOptions);
        _logger.LogDebug("Successfully parsed current season info from ESPN Core API");
        
        return seasonResponse;
    }

    /// <summary>
    /// Executes HTTP request with retry logic and intelligent error handling
    /// </summary>
    /// <param name="endpoint">API endpoint to call</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response content as string</returns>
    private async Task<string?> ExecuteWithRetryAsync(string endpoint, CancellationToken cancellationToken)
    {
        var attempt = 0;
        
        while (attempt <= _settings.MaxRetries)
        {
            try
            {
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogDebug("ESPN API request successful: {Endpoint} (Attempt {Attempt})", endpoint, attempt + 1);
                    return content;
                }
                
                // Handle specific ESPN API errors
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("ESPN API returned bad request for {Endpoint}: {ErrorContent}", endpoint, errorContent);
                    
                    // Don't retry bad requests - they indicate invalid parameters
                    return null;
                }
                
                // Handle rate limiting
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta?.TotalMilliseconds ?? _settings.RetryDelayMs;
                    _logger.LogWarning("Rate limited by ESPN API. Waiting {RetryAfter}ms before retry", retryAfter);
                    await Task.Delay(TimeSpan.FromMilliseconds(retryAfter), cancellationToken);
                }
                else
                {
                    _logger.LogWarning("ESPN API request failed: {Endpoint} - Status: {StatusCode} (Attempt {Attempt})", 
                        endpoint, response.StatusCode, attempt + 1);
                }
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("ESPN API request cancelled: {Endpoint}", endpoint);
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "HTTP error during ESPN API request: {Endpoint} (Attempt {Attempt})", endpoint, attempt + 1);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "ESPN API request timed out: {Endpoint} (Attempt {Attempt})", endpoint, attempt + 1);
            }

            attempt++;
            
            // Don't retry bad requests
            if (attempt <= _settings.MaxRetries)
            {
                var delay = _settings.RetryDelayMs * attempt; // Exponential backoff
                _logger.LogInformation("Retrying ESPN API request in {Delay}ms: {Endpoint} (Attempt {Attempt})", 
                    delay, endpoint, attempt + 1);
                await Task.Delay(delay, cancellationToken);
            }
        }

        _logger.LogError("ESPN API request failed after {MaxRetries} attempts: {Endpoint}", _settings.MaxRetries, endpoint);
        return null;
    }

    /// <summary>
    /// Determines if a season is currently active based on its JSON data
    /// </summary>
    /// <param name="seasonJson">Season JSON from ESPN Core API</param>
    /// <returns>True if season is currently active</returns>
    private static bool IsSeasonCurrentlyActive(string seasonJson)
    {
        try
        {
            var now = DateTime.UtcNow;
            var doc = JsonDocument.Parse(seasonJson);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("startDate", out var startDateElement) &&
                root.TryGetProperty("endDate", out var endDateElement))
            {
                var startDateStr = startDateElement.GetString();
                var endDateStr = endDateElement.GetString();
                
                if (DateTime.TryParse(startDateStr, out var startDate) && 
                    DateTime.TryParse(endDateStr, out var endDate))
                {
                    return now >= startDate && now <= endDate;
                }
            }
        }
        catch
        {
            // If we can't parse the JSON, assume it's not active
        }
        
        return false;
    }
}