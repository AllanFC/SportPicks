using Application.Common.Interfaces;
using Domain.Sports;
using Infrastructure.ExternalApis.Espn.Dtos;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace Infrastructure.Services;

/// <summary>
/// Service for synchronizing NFL data from ESPN APIs using the new multi-sport structure
/// </summary>
public sealed class NflDataSyncService : INflDataSyncService
{
    private readonly IEspnApiClient _espnApiClient;
    private readonly ICompetitorRepository _competitorRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ISeasonRepository _seasonRepository;
    private readonly ISeasonSyncService _seasonSyncService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NflDataSyncService> _logger;
    private readonly NflSyncSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    // NFL Sport ID - matches the one in migration
    private static readonly Guid NflSportId = new("11111111-1111-1111-1111-111111111111");

    public NflDataSyncService(
        IEspnApiClient espnApiClient,
        ICompetitorRepository competitorRepository,
        IEventRepository eventRepository,
        ISeasonRepository seasonRepository,
        ISeasonSyncService seasonSyncService,
        ApplicationDbContext context,
        ILogger<NflDataSyncService> logger,
        IOptions<NflSyncSettings> settings)
    {
        _espnApiClient = espnApiClient ?? throw new ArgumentNullException(nameof(espnApiClient));
        _competitorRepository = competitorRepository ?? throw new ArgumentNullException(nameof(competitorRepository));
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        _seasonRepository = seasonRepository ?? throw new ArgumentNullException(nameof(seasonRepository));
        _seasonSyncService = seasonSyncService ?? throw new ArgumentNullException(nameof(seasonSyncService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        
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
            // Ensure NFL sport exists before syncing teams
            await EnsureNflSportExistsAsync(cancellationToken);

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
            var competitors = MapEspnTeamsToCompetitors(espnTeams);

            await _competitorRepository.AddOrUpdateRangeAsync(competitors, cancellationToken);

            _logger.LogInformation("Successfully synchronized {Count} NFL teams", competitors.Count);
            return competitors.Count;
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
        // Get exact sync date range from database/ESPN Core API (no buffers)
        var (startDate, endDate) = await GetSyncDateRangeAsync(cancellationToken);
        return await SyncMatchesAsync(startDate, endDate, cancellationToken);
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

            var events = await MapEspnEventsToEventsAsync(response.Events, response.Season, response.Week, cancellationToken);

            await _eventRepository.AddOrUpdateRangeAsync(events, cancellationToken);

            _logger.LogInformation("Successfully synchronized {Count} NFL matches for date range {StartDate} to {EndDate}", 
                events.Count, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
            return events.Count;
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
            var seasonEntity = await _seasonRepository.GetByYearAndSportAsync(season, NflSportId, cancellationToken);
            if (seasonEntity == null)
            {
                _logger.LogWarning("Season {Season} not found for NFL, attempting to sync", season);
                await _seasonSyncService.SyncSeasonAsync(season, cancellationToken);
                seasonEntity = await _seasonRepository.GetByYearAndSportAsync(season, NflSportId, cancellationToken);
                
                if (seasonEntity == null)
                {
                    throw new InvalidOperationException($"Unable to find or create NFL season {season}");
                }
            }

            return await SyncMatchesAsync(seasonEntity.StartDate, seasonEntity.EndDate, cancellationToken);
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
            // First ensure we have current season data
            await _seasonSyncService.SyncCurrentSeasonAsync(cancellationToken);

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
    /// Gets exact sync date range from database/ESPN Core API (no arbitrary buffers)
    /// </summary>
    private async Task<(DateTime StartDate, DateTime EndDate)> GetSyncDateRangeAsync(CancellationToken cancellationToken)
    {
        try
        {
            // First try to sync current season to ensure we have up-to-date data
            var currentSeason = await _seasonSyncService.SyncCurrentSeasonAsync(cancellationToken);
            
            if (currentSeason != null)
            {
                _logger.LogInformation("Using current season dates: {Start} to {End}", 
                    currentSeason.StartDate.ToString("yyyy-MM-dd"), currentSeason.EndDate.ToString("yyyy-MM-dd"));
                
                return (currentSeason.StartDate, currentSeason.EndDate);
            }

            // Fallback to active NFL season
            var activeSeason = await _seasonRepository.GetActiveByYearAndSportAsync(DateTime.UtcNow.Year, NflSportId, cancellationToken);
            if (activeSeason != null)
            {
                _logger.LogInformation("Using active season dates: {Start} to {End}", 
                    activeSeason.StartDate.ToString("yyyy-MM-dd"), activeSeason.EndDate.ToString("yyyy-MM-dd"));
                
                return (activeSeason.StartDate, activeSeason.EndDate);
            }

            // Very last resort: current year estimate
            var currentYear = DateTime.Now.Year;
            var fallbackStart = new DateTime(currentYear, 8, 1);
            var fallbackEnd = new DateTime(currentYear + 1, 2, 28);
            
            _logger.LogWarning("Using fallback date range: {Start} to {End}", 
                fallbackStart.ToString("yyyy-MM-dd"), fallbackEnd.ToString("yyyy-MM-dd"));
                
            return (fallbackStart, fallbackEnd);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get season dates, using current year estimate");
            
            var currentYear = DateTime.Now.Year;
            var fallbackStart = new DateTime(currentYear, 8, 1);
            var fallbackEnd = new DateTime(currentYear + 1, 2, 28);
            
            return (fallbackStart, fallbackEnd);
        }
    }

    /// <summary>
    /// Maps ESPN teams DTOs to Competitor entities
    /// </summary>
    private List<Competitor> MapEspnTeamsToCompetitors(List<EspnTeam> espnTeams)
    {
        ArgumentNullException.ThrowIfNull(espnTeams);
        
        var competitors = new List<Competitor>();

        foreach (var espnTeam in espnTeams)
        {
            try
            {
                var teamDetails = espnTeam.Team;
                if (string.IsNullOrWhiteSpace(teamDetails?.Id) || 
                    string.IsNullOrWhiteSpace(teamDetails?.DisplayName))
                {
                    _logger.LogWarning("Skipping invalid team data from ESPN API");
                    continue;
                }

                var competitor = new Competitor(
                    teamDetails.DisplayName,
                    teamDetails.Abbreviation ?? teamDetails.DisplayName.Substring(0, Math.Min(3, teamDetails.DisplayName.Length)).ToUpper(),
                    NflSportId)
                {
                    Location = teamDetails.Location,
                    Nickname = teamDetails.Nickname,
                    LogoUrl = teamDetails.Logos?.FirstOrDefault()?.Href,
                    Color = teamDetails.Color,
                    AlternateColor = teamDetails.AlternateColor,
                    IsActive = teamDetails.IsActive
                };

                competitor.SetExternalReference(teamDetails.Id, "ESPN");

                competitors.Add(competitor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to map team {TeamId} from ESPN API", espnTeam.Team?.Id);
            }
        }

        return competitors;
    }

    /// <summary>
    /// Maps ESPN events DTOs to Event entities with proper EventCompetitor handling for both inserts and updates
    /// </summary>
    private async Task<List<Event>> MapEspnEventsToEventsAsync(List<EspnEvent> espnEvents, EspnSeason? defaultSeason = null, EspnWeek? defaultWeek = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(espnEvents);
        
        var events = new List<Event>();

        foreach (var espnEvent in espnEvents)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(espnEvent.Id) || 
                    string.IsNullOrWhiteSpace(espnEvent.Name) ||
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

                if (string.IsNullOrWhiteSpace(homeTeam?.Team?.Id) || string.IsNullOrWhiteSpace(awayTeam?.Team?.Id))
                {
                    _logger.LogWarning("Could not identify home/away teams for event {EventId}, skipping", espnEvent.Id);
                    continue;
                }

                // Parse date
                if (!DateTime.TryParse(espnEvent.Date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var eventDate))
                {
                    _logger.LogWarning("Could not parse date {Date} for event {EventId}, skipping", espnEvent.Date, espnEvent.Id);
                    continue;
                }

                // Get season info
                var season = espnEvent.Season ?? defaultSeason;
                var week = espnEvent.Week?.Number ?? defaultWeek?.Number ?? 1;
                var eventYear = season?.Year ?? DateTime.UtcNow.Year;

                // Get the season entity
                var seasonEntity = await _seasonRepository.GetByYearAndSportAsync(eventYear, NflSportId, cancellationToken);
                if (seasonEntity == null)
                {
                    _logger.LogWarning("Season {Year} not found for NFL, skipping event {EventId}", eventYear, espnEvent.Id);
                    continue;
                }

                var eventEntity = new Event(espnEvent.Name, seasonEntity.Id, eventDate, espnEvent.Status?.Type?.State ?? "unknown")
                {
                    IsCompleted = espnEvent.Status?.Type?.Completed ?? false,
                    Venue = competition.Venue?.FullName,
                    Week = week,
                    EventType = season?.Slug ?? "regular"
                };

                eventEntity.SetExternalReference(espnEvent.Id, "ESPN");

                // Add competitor relationships with proper winner calculation
                var homeCompetitor = await _competitorRepository.GetByExternalIdAsync(homeTeam.Team.Id, "ESPN", cancellationToken);
                var awayCompetitor = await _competitorRepository.GetByExternalIdAsync(awayTeam.Team.Id, "ESPN", cancellationToken);

                if (homeCompetitor != null && awayCompetitor != null)
                {
                    var homeEventCompetitor = new EventCompetitor(eventEntity.Id, homeCompetitor.Id)
                    {
                        IsHomeTeam = true
                    };

                    var awayEventCompetitor = new EventCompetitor(eventEntity.Id, awayCompetitor.Id)
                    {
                        IsHomeTeam = false
                    };

                    // Parse scores and determine winner
                    SetEventCompetitorResults(homeEventCompetitor, awayEventCompetitor, homeTeam, awayTeam, 
                                            eventEntity.IsCompleted, homeCompetitor.Name, awayCompetitor.Name, espnEvent.Id);

                    eventEntity.EventCompetitors.Add(homeEventCompetitor);
                    eventEntity.EventCompetitors.Add(awayEventCompetitor);
                }

                events.Add(eventEntity);

                _logger.LogDebug("Mapped event {EventId}: Season {Season}, Week {Week}", 
                    espnEvent.Id, eventYear, week);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to map event {EventId} from ESPN API", espnEvent.Id);
            }
        }

        return events;
    }

    /// <summary>
    /// Sets EventCompetitor results including scores and winner determination
    /// This method handles both new events and updates to existing events
    /// </summary>
    private void SetEventCompetitorResults(EventCompetitor homeEventCompetitor, EventCompetitor awayEventCompetitor, 
                                         EspnCompetitor homeTeam, EspnCompetitor awayTeam, 
                                         bool isCompleted, string homeTeamName, string awayTeamName, string eventId)
    {
        ArgumentNullException.ThrowIfNull(homeEventCompetitor);
        ArgumentNullException.ThrowIfNull(awayEventCompetitor);
        ArgumentNullException.ThrowIfNull(homeTeam);
        ArgumentNullException.ThrowIfNull(awayTeam);
        
        // Parse scores
        int? homeScore = null;
        int? awayScore = null;

        if (int.TryParse(homeTeam.Score, out var parsedHomeScore))
        {
            homeScore = parsedHomeScore;
        }

        if (int.TryParse(awayTeam.Score, out var parsedAwayScore))
        {
            awayScore = parsedAwayScore;
        }

        // Update results using the EventCompetitor's UpdateResult method
        // This ensures both new and existing EventCompetitors get proper values
        if (isCompleted)
        {
            // Use ESPN's winner field if available (most reliable)
            if (homeTeam.Winner.HasValue && awayTeam.Winner.HasValue)
            {
                homeEventCompetitor.UpdateResult(score: homeScore, isWinner: homeTeam.Winner.Value);
                awayEventCompetitor.UpdateResult(score: awayScore, isWinner: awayTeam.Winner.Value);
                
                _logger.LogDebug("Game {EventId} completed (ESPN winner): {HomeTeam} {HomeScore} - {AwayTeam} {AwayScore} (Winner: {Winner})", 
                    eventId, homeTeamName, homeScore?.ToString() ?? "N/A", awayTeamName, awayScore?.ToString() ?? "N/A",
                    homeTeam.Winner.Value ? homeTeamName : awayTeam.Winner.Value ? awayTeamName : "Tie");
            }
            // Fallback to score comparison if ESPN winner field is not available
            else if (homeScore.HasValue && awayScore.HasValue)
            {
                if (homeScore.Value > awayScore.Value)
                {
                    homeEventCompetitor.UpdateResult(score: homeScore, isWinner: true);
                    awayEventCompetitor.UpdateResult(score: awayScore, isWinner: false);
                }
                else if (awayScore.Value > homeScore.Value)
                {
                    homeEventCompetitor.UpdateResult(score: homeScore, isWinner: false);
                    awayEventCompetitor.UpdateResult(score: awayScore, isWinner: true);
                }
                else
                {
                    // Tie game - both are not winners
                    homeEventCompetitor.UpdateResult(score: homeScore, isWinner: false);
                    awayEventCompetitor.UpdateResult(score: awayScore, isWinner: false);
                }
                
                var winner = homeScore.Value > awayScore.Value ? homeTeamName : 
                            awayScore.Value > homeScore.Value ? awayTeamName : "Tie";
                
                _logger.LogDebug("Game {EventId} completed (score comparison): {HomeTeam} {HomeScore} - {AwayTeam} {AwayScore} (Winner: {Winner})", 
                    eventId, homeTeamName, homeScore, awayTeamName, awayScore, winner);
            }
            else
            {
                // Game completed but no scores or winner info available
                homeEventCompetitor.UpdateResult(score: homeScore, isWinner: null);
                awayEventCompetitor.UpdateResult(score: awayScore, isWinner: null);
                
                _logger.LogWarning("Game {EventId} marked as completed but no winner or score data available: {HomeTeam} vs {AwayTeam}", 
                    eventId, homeTeamName, awayTeamName);
            }
        }
        else
        {
            // Game not completed - set scores but leave IsWinner as null
            homeEventCompetitor.UpdateResult(score: homeScore, isWinner: null);
            awayEventCompetitor.UpdateResult(score: awayScore, isWinner: null);
            
            _logger.LogDebug("Game {EventId} not completed: {HomeTeam} vs {AwayTeam} (Scores: {HomeScore}-{AwayScore})", 
                eventId, homeTeamName, awayTeamName, homeScore?.ToString() ?? "N/A", awayScore?.ToString() ?? "N/A");
        }
    }

    /// <summary>
    /// Ensures NFL sport record exists in the database
    /// </summary>
    private async Task EnsureNflSportExistsAsync(CancellationToken cancellationToken)
    {
        var existingSport = await _context.Sports
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == NflSportId, cancellationToken);
            
        if (existingSport == null)
        {
            var nflSport = new Sport("National Football League", "NFL")
            {
                Id = NflSportId,
                Description = "American professional football league",
                IsActive = true
            };
            
            _context.Sports.Add(nflSport);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created NFL sport record");
        }
    }
}