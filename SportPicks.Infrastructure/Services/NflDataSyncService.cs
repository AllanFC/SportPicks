using Infrastructure.ExternalApis.Espn.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace Infrastructure.Services;

/// <summary>
/// Service for synchronizing NFL data from ESPN APIs
/// </summary>
public class NflDataSyncService : INflDataSyncService
{
    private readonly IEspnApiClient _espnApiClient;
    private readonly ITeamRepository _teamRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly INflSeasonService _seasonService;
    private readonly ILogger<NflDataSyncService> _logger;
    private readonly NflSyncSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public NflDataSyncService(
        IEspnApiClient espnApiClient,
        ITeamRepository teamRepository,
        IMatchRepository matchRepository,
        INflSeasonService seasonService,
        ILogger<NflDataSyncService> logger,
        IOptions<NflSyncSettings> settings)
    {
        _espnApiClient = espnApiClient;
        _teamRepository = teamRepository;
        _matchRepository = matchRepository;
        _seasonService = seasonService;
        _logger = logger;
        _settings = settings.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc />
    public async Task<int> SyncTeamsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting NFL teams synchronization");

        try
        {
            var json = await _espnApiClient.GetTeamsJsonAsync(cancellationToken);
            if (string.IsNullOrEmpty(json))
            {
                _logger.LogWarning("No teams data received from ESPN API");
                return 0;
            }

            var response = JsonSerializer.Deserialize<EspnTeamsResponse>(json, _jsonOptions);
            if (response?.Sports?.FirstOrDefault()?.Leagues?.FirstOrDefault()?.Teams == null)
            {
                _logger.LogWarning("No teams data found in ESPN API response");
                return 0;
            }

            var espnTeams = response.Sports.First().Leagues.First().Teams;
            var teams = MapEspnTeamsToEntities(espnTeams);

            await _teamRepository.AddOrUpdateRangeAsync(teams, cancellationToken);

            _logger.LogInformation("Successfully synchronized {Count} NFL teams", teams.Count);
            return teams.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to synchronize NFL teams");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> SyncMatchesAsync(CancellationToken cancellationToken = default)
    {
        return await SyncMatchesAsync(_settings.SyncStartDate, _settings.SyncEndDate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> SyncMatchesAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting NFL matches synchronization for date range {StartDate} to {EndDate}", 
            startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

        try
        {
            var json = await _espnApiClient.GetScoreboardJsonAsync(startDate, endDate, cancellationToken);
            if (string.IsNullOrEmpty(json))
            {
                _logger.LogWarning("No scoreboard data received from ESPN API for date range {StartDate} to {EndDate}", 
                    startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
                return 0;
            }

            var response = JsonSerializer.Deserialize<EspnScoreboardResponse>(json, _jsonOptions);
            if (response?.Events == null)
            {
                _logger.LogWarning("No events data found in ESPN API response for date range {StartDate} to {EndDate}", 
                    startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
                return 0;
            }

            // Get current season as fallback
            var fallbackSeason = await _seasonService.GetCurrentSeasonAsync(cancellationToken);
            var matches = await MapEspnEventsToEntitiesAsync(response.Events, response.Season, response.Week, fallbackSeason);

            await _matchRepository.AddOrUpdateRangeAsync(matches, cancellationToken);

            _logger.LogInformation("Successfully synchronized {Count} NFL matches for date range {StartDate} to {EndDate}", 
                matches.Count, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
            return matches.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to synchronize NFL matches for date range {StartDate} to {EndDate}", 
                startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> SyncMatchesForSeasonAsync(int season, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting NFL matches synchronization for season {Season}", season);

        try
        {
            var (startDate, endDate) = await _seasonService.GetSeasonDateRangeAsync(season, cancellationToken);
            return await SyncMatchesAsync(startDate, endDate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to synchronize NFL matches for season {Season}", season);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<(int TeamsSynced, int MatchesSynced)> PerformFullSyncAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting full NFL data synchronization");

        try
        {
            var teamsSynced = await SyncTeamsAsync(cancellationToken);
            var matchesSynced = await SyncMatchesAsync(cancellationToken);

            _logger.LogInformation("Full NFL synchronization completed: {TeamCount} teams, {MatchCount} matches", 
                teamsSynced, matchesSynced);

            return (teamsSynced, matchesSynced);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform full NFL data synchronization");
            throw;
        }
    }

    /// <summary>
    /// Maps ESPN teams DTOs to domain Team entities
    /// </summary>
    /// <param name="espnTeams">ESPN teams from API</param>
    /// <returns>List of Team entities</returns>
    private List<Team> MapEspnTeamsToEntities(List<EspnTeam> espnTeams)
    {
        var teams = new List<Team>();

        foreach (var espnTeam in espnTeams)
        {
            try
            {
                var teamDetails = espnTeam.Team;
                if (string.IsNullOrEmpty(teamDetails.Id) || 
                    string.IsNullOrEmpty(teamDetails.DisplayName))
                {
                    _logger.LogWarning("Skipping invalid team data from ESPN API");
                    continue;
                }

                var team = new Team(
                    teamDetails.Id,
                    teamDetails.DisplayName,
                    teamDetails.Abbreviation,
                    teamDetails.Location,
                    teamDetails.Nickname);

                // Set optional properties
                team.LogoUrl = teamDetails.Logos?.FirstOrDefault()?.Href;
                team.Color = teamDetails.Color;
                team.AlternateColor = teamDetails.AlternateColor;
                team.IsActive = teamDetails.IsActive;

                teams.Add(team);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to map team {TeamId} from ESPN API", espnTeam.Team?.Id);
            }
        }

        return teams;
    }

    /// <summary>
    /// Maps ESPN events DTOs to domain Match entities (async to handle season fallback)
    /// </summary>
    /// <param name="espnEvents">ESPN events from API</param>
    /// <param name="defaultSeason">Default season from response (fallback)</param>
    /// <param name="defaultWeek">Default week from response (fallback)</param>
    /// <param name="fallbackSeason">Fallback season year if no season data available</param>
    /// <returns>List of Match entities</returns>
    private async Task<List<Domain.Sports.Match>> MapEspnEventsToEntitiesAsync(List<EspnEvent> espnEvents, EspnSeason? defaultSeason = null, EspnWeek? defaultWeek = null, int fallbackSeason = 2024)
    {
        var matches = new List<Domain.Sports.Match>();

        foreach (var espnEvent in espnEvents)
        {
            try
            {
                if (string.IsNullOrEmpty(espnEvent.Id) || 
                    string.IsNullOrEmpty(espnEvent.Name) ||
                    espnEvent.Competitions?.FirstOrDefault()?.Competitors == null)
                {
                    _logger.LogWarning("Skipping invalid event data from ESPN API: {EventId}", espnEvent.Id);
                    continue;
                }

                var competition = espnEvent.Competitions.First();
                var competitors = competition.Competitors;

                if (competitors.Count != 2)
                {
                    _logger.LogWarning("Event {EventId} does not have exactly 2 competitors, skipping", espnEvent.Id);
                    continue;
                }

                var homeTeam = competitors.FirstOrDefault(c => c.HomeAway.Equals("home", StringComparison.OrdinalIgnoreCase));
                var awayTeam = competitors.FirstOrDefault(c => c.HomeAway.Equals("away", StringComparison.OrdinalIgnoreCase));

                if (homeTeam?.Team?.Id == null || awayTeam?.Team?.Id == null)
                {
                    _logger.LogWarning("Could not identify home/away teams for event {EventId}, skipping", espnEvent.Id);
                    continue;
                }

                // Parse date
                if (!DateTime.TryParse(espnEvent.Date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var matchDate))
                {
                    _logger.LogWarning("Could not parse date {Date} for event {EventId}, skipping", espnEvent.Date, espnEvent.Id);
                    continue;
                }

                // Use ESPN's official season and week data (event-specific first, then response default, then fallback)
                var season = espnEvent.Season ?? defaultSeason;
                var week = espnEvent.Week?.Number ?? defaultWeek?.Number ?? 1;
                
                var eventSeason = season?.Year ?? fallbackSeason;
                var seasonType = season?.Type ?? 2; // Default to regular season
                var seasonTypeSlug = season?.Slug ?? "regular";

                var match = new Domain.Sports.Match(
                    espnEvent.Id,
                    espnEvent.Name,
                    matchDate,
                    eventSeason,
                    seasonType,
                    seasonTypeSlug,
                    week,
                    homeTeam.Team.Id,
                    awayTeam.Team.Id,
                    espnEvent.Status?.Type?.State ?? "unknown",
                    espnEvent.Status?.Type?.Completed ?? false);

                // Set optional properties
                if (int.TryParse(homeTeam.Score, out var homeScore))
                    match.HomeScore = homeScore;

                if (int.TryParse(awayTeam.Score, out var awayScore))
                    match.AwayScore = awayScore;

                match.Venue = competition.Venue?.FullName;

                matches.Add(match);
                
                _logger.LogDebug("Mapped match {EventId}: {SeasonTypeSlug} Season {Season}, Week {Week}", 
                    espnEvent.Id, seasonTypeSlug, eventSeason, week);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to map event {EventId} from ESPN API", espnEvent.Id);
            }
        }

        return matches;
    }
}