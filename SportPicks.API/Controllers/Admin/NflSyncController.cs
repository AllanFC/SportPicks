using Application.NflSync.Dtos;

namespace SportPicks.API.Controllers.Admin;

/// <summary>
/// Controller for NFL data synchronization operations
/// </summary>
[ApiController]
[Route("api/v1/admin/nfl-sync")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Tags("NFL Data Synchronization")]
[Produces("application/json")]
public class NflSyncController : ControllerBase
{
    private readonly INflDataSyncService _nflDataSyncService;
    private readonly ILogger<NflSyncController> _logger;

    public NflSyncController(INflDataSyncService nflDataSyncService, ILogger<NflSyncController> logger)
    {
        _nflDataSyncService = nflDataSyncService;
        _logger = logger;
    }

    /// <summary>
    /// Synchronizes NFL teams from ESPN API
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result with team count</returns>
    /// <response code="200">Teams synchronized successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("teams")]
    [Authorize(Policy = AuthorizationPolicies.NflDataSync)]
    [ProducesResponseType<TeamSyncResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TeamSyncResponseDto>> SyncTeams(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("NFL teams sync requested by user {UserName} ({UserId})", userName, userId);

        var teamCount = await _nflDataSyncService.SyncTeamsAsync(cancellationToken);

        _logger.LogInformation("NFL teams sync completed successfully. {TeamCount} teams synced by {UserName} ({UserId})",
            teamCount, userName, userId);

        return Ok(new TeamSyncResponseDto
        {
            Success = true,
            Message = "Teams synchronized successfully",
            TeamCount = teamCount,
            SyncedAt = DateTime.UtcNow,
            SyncedBy = userName
        });
    }

    /// <summary>
    /// Synchronizes NFL matches for the current season
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result with match count</returns>
    /// <response code="200">Matches synchronized successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("matches")]
    [Authorize(Policy = AuthorizationPolicies.NflDataSync)]
    [ProducesResponseType<MatchSyncResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MatchSyncResponseDto>> SyncMatches(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("NFL matches sync requested by user {UserName} ({UserId})", userName, userId);

        var matchCount = await _nflDataSyncService.SyncMatchesAsync(cancellationToken);

        _logger.LogInformation("NFL matches sync completed successfully. {MatchCount} matches synced by {UserName} ({UserId})",
            matchCount, userName, userId);

        return Ok(new MatchSyncResponseDto
        {
            Success = true,
            Message = "Matches synchronized successfully",
            MatchCount = matchCount,
            SyncedAt = DateTime.UtcNow,
            SyncedBy = userName
        });
    }

    /// <summary>
    /// Synchronizes NFL matches for a specific date range
    /// </summary>
    /// <param name="request">Date range parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result with match count</returns>
    /// <response code="200">Matches synchronized successfully</response>
    /// <response code="400">Bad request - Invalid date format or range</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("matches/date-range")]
    [Authorize(Policy = AuthorizationPolicies.NflDataSync)]
    [ProducesResponseType<MatchSyncResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MatchSyncResponseDto>> SyncMatchesByDateRange(
        [FromQuery] DateRangeSyncRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Validate input using the DTO's validation method
        var (isValid, start, end, errorMessage) = request.ValidateAndParse();
        if (!isValid)
        {
            return BadRequest(new ErrorResponseDto { Message = errorMessage! });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("NFL matches sync by date range requested: {StartDate} to {EndDate} by user {UserName} ({UserId})",
            start!.Value.ToString("yyyy-MM-dd"), end!.Value.ToString("yyyy-MM-dd"), userName, userId);

        var matchCount = await _nflDataSyncService.SyncMatchesAsync(start.Value, end.Value, cancellationToken);

        _logger.LogInformation("NFL matches sync by date range completed successfully. {MatchCount} matches synced by {UserName} ({UserId})",
            matchCount, userName, userId);

        return Ok(new MatchSyncResponseDto
        {
            Success = true,
            Message = "Matches synchronized successfully",
            MatchCount = matchCount,
            StartDate = start.Value.ToString("yyyy-MM-dd"),
            EndDate = end.Value.ToString("yyyy-MM-dd"),
            SyncedAt = DateTime.UtcNow,
            SyncedBy = userName
        });
    }

    /// <summary>
    /// Synchronizes NFL matches for a specific season
    /// </summary>
    /// <param name="season">Season year (e.g., 2024)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result with match count</returns>
    /// <response code="200">Matches synchronized successfully</response>
    /// <response code="400">Bad request - Invalid season year</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("matches/season/{season:int}")]
    [Authorize(Policy = AuthorizationPolicies.NflDataSync)]
    [ProducesResponseType<MatchSyncResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MatchSyncResponseDto>> SyncMatchesForSeason(
        [FromRoute] int season,
        CancellationToken cancellationToken = default)
    {
        // Validate season using proper DTO validation
        var seasonRequest = new SeasonSyncRequestDto { Season = season };
        var (isValid, errorMessage) = seasonRequest.Validate();
        if (!isValid)
        {
            return BadRequest(new ErrorResponseDto { Message = errorMessage! });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("NFL matches sync for season {Season} requested by user {UserName} ({UserId})",
            season, userName, userId);

        var matchCount = await _nflDataSyncService.SyncMatchesForSeasonAsync(season, cancellationToken);

        _logger.LogInformation("NFL matches sync for season {Season} completed successfully. {MatchCount} matches synced by {UserName} ({UserId})",
            season, matchCount, userName, userId);

        return Ok(new MatchSyncResponseDto
        {
            Success = true,
            Message = $"Matches for season {season} synchronized successfully",
            MatchCount = matchCount,
            Season = season,
            SyncedAt = DateTime.UtcNow,
            SyncedBy = userName
        });
    }

    /// <summary>
    /// Performs a full synchronization of both teams and matches (HIGH IMPACT OPERATION)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result with both team and match counts</returns>
    /// <remarks>
    /// This is a high-impact operation that synchronizes all NFL teams and matches.
    /// Requires Admin privileges and will be logged extensively for audit purposes.
    /// 
    /// Sample request:
    /// ```
    /// POST /api/v1/admin/nfl-sync/full
    /// Authorization: Bearer {your-jwt-token}
    /// ```
    /// 
    /// Sample response:
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Full synchronization completed successfully",
    ///   "teamCount": 32,
    ///   "matchCount": 285,
    ///   "syncedAt": "2024-01-25T14:30:00Z",
    ///   "syncedBy": "admin@example.com",
    ///   "isHighImpactOperation": true
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Full synchronization completed successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role with high-impact permissions required</response>
    /// <response code="500">Internal server error during synchronization</response>
    [HttpPost("full")]
    [Authorize(Policy = AuthorizationPolicies.HighImpactOperations)]
    [ProducesResponseType<FullSyncResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FullSyncResponseDto>> FullSync(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        _logger.LogWarning("HIGH-IMPACT OPERATION: Full NFL data sync requested by user {UserName} ({UserId}) - {Email}",
            userName, userId, userEmail);

        var (teamCount, matchCount) = await _nflDataSyncService.PerformFullSyncAsync(cancellationToken);

        _logger.LogWarning("HIGH-IMPACT OPERATION COMPLETED: Full NFL data sync by {UserName} ({UserId}) - Teams: {TeamCount}, Matches: {MatchCount}",
            userName, userId, teamCount, matchCount);

        return Ok(new FullSyncResponseDto
        {
            Success = true,
            Message = "Full synchronization completed successfully",
            TeamCount = teamCount,
            MatchCount = matchCount,
            SyncedAt = DateTime.UtcNow,
            SyncedBy = userName,
            IsHighImpactOperation = true
        });
    }
}