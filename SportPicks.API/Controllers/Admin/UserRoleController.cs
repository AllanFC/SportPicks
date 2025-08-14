using SportPicks.API.Models;

namespace SportPicks.API.Controllers.Admin;

/// <summary>
/// Controller for user role management operations (Development/Admin use)
/// </summary>
[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Tags("User Management")]
[Produces("application/json")]
public sealed class UserRoleController : ControllerBase
{
    private readonly IUserRoleService _userRoleService;
    private readonly ILogger<UserRoleController> _logger;

    public UserRoleController(IUserRoleService userRoleService, ILogger<UserRoleController> logger)
    {
        _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Promotes a user to Admin role by email (USE WITH CAUTION)
    /// </summary>
    /// <param name="email">Email of user to promote</param>
    /// <returns>Success result</returns>
    /// <response code="200">User promoted to admin successfully</response>
    /// <response code="400">Bad request - email missing or user not found</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("promote-to-admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PromoteToAdmin([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new ErrorResponse { Message = "Email is required" });
        }

        var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminUserName = User.FindFirst(ClaimTypes.Name)?.Value;
        
        _logger.LogWarning("ADMIN PROMOTION REQUEST: User {AdminUserName} ({AdminUserId}) attempting to promote {TargetEmail} to admin", 
            adminUserName, adminUserId, email);

        try
        {
            var success = await _userRoleService.PromoteToAdminAsync(email);
            
            if (success)
            {
                _logger.LogWarning("ADMIN PROMOTION SUCCESS: User {TargetEmail} promoted to admin by {AdminUserName} ({AdminUserId})", 
                    email, adminUserName, adminUserId);
                
                return Ok(new
                {
                    Success = true,
                    Message = $"User {email} promoted to admin successfully",
                    PromotedBy = adminUserName,
                    PromotedAt = DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogWarning("ADMIN PROMOTION FAILED: Could not promote {TargetEmail} - user not found or already admin", email);
                return BadRequest(new ErrorResponse { Message = "Failed to promote user - user not found or already admin" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error promoting user {Email} to admin", email);
            return StatusCode(500, new ErrorResponse { Message = "Internal server error occurred while promoting user" });
        }
    }

    /// <summary>
    /// Gets all admin users in the system
    /// </summary>
    /// <returns>List of admin users</returns>
    /// <response code="200">Admin users retrieved successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("admins")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAdminUsers()
    {
        try
        {
            var adminUsers = await _userRoleService.GetAdminUsersAsync();
            
            var adminData = adminUsers.Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.UserRole,
                u.CreatedAt,
                u.UpdatedAt
            }).ToList();

            return Ok(new
            {
                Success = true,
                AdminCount = adminData.Count,
                Admins = adminData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin users");
            return StatusCode(500, new ErrorResponse { Message = "Internal server error occurred while retrieving admin users" });
        }
    }

    /// <summary>
    /// Demotes an admin user to regular user (USE WITH EXTREME CAUTION)
    /// </summary>
    /// <param name="userId">User ID to demote</param>
    /// <returns>Success result</returns>
    /// <response code="200">Admin user demoted successfully</response>
    /// <response code="400">Bad request - cannot demote yourself or user not found</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("demote-admin/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DemoteAdmin(Guid userId)
    {
        var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminUserName = User.FindFirst(ClaimTypes.Name)?.Value;
        
        // Prevent self-demotion
        if (adminUserId == userId.ToString())
        {
            return BadRequest(new ErrorResponse { Message = "Cannot demote yourself" });
        }

        _logger.LogWarning("ADMIN DEMOTION REQUEST: User {AdminUserName} ({AdminUserId}) attempting to demote admin {TargetUserId}", 
            adminUserName, adminUserId, userId);

        try
        {
            var success = await _userRoleService.DemoteToUserAsync(userId);
            
            if (success)
            {
                _logger.LogWarning("ADMIN DEMOTION SUCCESS: Admin {TargetUserId} demoted to user by {AdminUserName} ({AdminUserId})", 
                    userId, adminUserName, adminUserId);
                
                return Ok(new
                {
                    Success = true,
                    Message = "Admin user demoted successfully",
                    DemotedBy = adminUserName,
                    DemotedAt = DateTime.UtcNow
                });
            }
            else
            {
                return BadRequest(new ErrorResponse { Message = "Failed to demote admin - user not found or not an admin" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error demoting admin user {UserId}", userId);
            return StatusCode(500, new ErrorResponse { Message = "Internal server error occurred while demoting admin" });
        }
    }
}