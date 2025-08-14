using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportPicks.API.Controllers.Users.Models;
using SportPicks.API.Models;

namespace SportPicks.API.Controllers.Users;

/// <summary>
/// Controller for user management operations
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Tags("User Management")]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="dto">User registration data</param>
    /// <returns>New user ID</returns>
    /// <response code="200">User created successfully</response>
    /// <response code="400">Bad request - validation failed or user already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] RegisterUserModel dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = await _userService.RegisterUserAsync(dto.Username, dto.Email, dto.Password);
            return Ok(new 
            { 
                Success = true,
                Message = "User registered successfully",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Updates a user's password
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="dto">Password update data</param>
    /// <returns>Success response</returns>
    /// <response code="200">Password updated successfully</response>
    /// <response code="400">Bad request - validation failed or invalid credentials</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{email}/password")]
    [AllowAnonymous] // Note: This should probably require authentication in a real app
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePassword(string email, [FromBody] UpdatePasswordModel dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.OldPassword == dto.NewPassword)
            return BadRequest(new ErrorResponse { Message = "New password must be different from the old password." });

        if (dto.NewPassword != dto.ConfirmPassword)
            return BadRequest(new ErrorResponse { Message = "Passwords do not match." });

        try
        {
            await _userService.UpdateUserPasswordAsync(email, dto.OldPassword, dto.NewPassword);
            return Ok(new 
            { 
                Success = true,
                Message = "Password updated successfully",
                UpdatedAt = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException)
        {
            return BadRequest(new ErrorResponse { Message = "Invalid email or current password." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
    }
}
