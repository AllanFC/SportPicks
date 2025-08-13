using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SportPicks.API.Authorization;

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
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> SyncTeams(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("NFL teams sync requested by user {UserName} ({UserId})", userName, userId);

        try
        {
            var teamCount = await _nflDataSyncService.SyncTeamsAsync(cancellationToken);

            _logger.LogInformation("NFL teams sync completed successfully. {TeamCount} teams synced by {UserName} ({UserId})",
                teamCount, userName, userId);

            return Ok(new
            {
                success = true,
                message = "Teams synchronized successfully",
                teamCount = teamCount,
                syncedAt = DateTime.UtcNow,
                syncedBy = userName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync NFL teams");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to sync NFL teams",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Synchronizes NFL matches/events for the current season
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result with match count</returns>
    /// <response code="200">Matches synchronized successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("events")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> SyncEvents(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("NFL events sync requested by user {UserName} ({UserId})", userName, userId);

        try
        {
            var eventCount = await _nflDataSyncService.SyncMatchesAsync(cancellationToken);

            _logger.LogInformation("NFL events sync completed successfully. {EventCount} events synced by {UserName} ({UserId})",
                eventCount, userName, userId);

            return Ok(new
            {
                success = true,
                message = "Events synchronized successfully",
                eventCount = eventCount,
                syncedAt = DateTime.UtcNow,
                syncedBy = userName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync NFL events");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to sync NFL events",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Synchronizes NFL matches/events for a specific season
    /// </summary>
    /// <param name="season">Season year (e.g., 2024)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result with match count</returns>
    /// <response code="200">Matches synchronized successfully</response>
    /// <response code="400">Bad request - Invalid season year</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("events/season/{season:int}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> SyncEventsForSeason(int season, CancellationToken cancellationToken = default)
    {
        // Basic validation
        if (season < 1990 || season > DateTime.UtcNow.Year + 2)
        {
            return BadRequest(new { message = "Invalid season year. Must be between 1990 and current year + 2." });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        _logger.LogInformation("NFL events sync for season {Season} requested by user {UserName} ({UserId})",
            season, userName, userId);

        try
        {
            var eventCount = await _nflDataSyncService.SyncMatchesForSeasonAsync(season, cancellationToken);

            _logger.LogInformation("NFL events sync for season {Season} completed successfully. {EventCount} events synced by {UserName} ({UserId})",
                season, eventCount, userName, userId);

            return Ok(new
            {
                success = true,
                message = $"Events for season {season} synchronized successfully",
                eventCount = eventCount,
                season = season,
                syncedAt = DateTime.UtcNow,
                syncedBy = userName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync NFL events for season {Season}", season);
            return StatusCode(500, new
            {
                success = false,
                message = $"Failed to sync NFL events for season {season}",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Performs a full synchronization of both teams and events (HIGH IMPACT OPERATION)
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
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> FullSync(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        _logger.LogWarning("HIGH-IMPACT OPERATION: Full NFL data sync requested by user {UserName} ({UserId}) - {Email}",
            userName, userId, userEmail);

        try
        {
            var (teamCount, eventCount) = await _nflDataSyncService.PerformFullSyncAsync(cancellationToken);

            _logger.LogWarning("HIGH-IMPACT OPERATION COMPLETED: Full NFL data sync by {UserName} ({UserId}) - Teams: {TeamCount}, Events: {EventCount}",
                userName, userId, teamCount, eventCount);

            return Ok(new
            {
                success = true,
                message = "Full synchronization completed successfully",
                teamCount = teamCount,
                eventCount = eventCount,
                syncedAt = DateTime.UtcNow,
                syncedBy = userName,
                isHighImpactOperation = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform full NFL sync");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to perform full NFL synchronization",
                error = ex.Message
            });
        }
    }
}