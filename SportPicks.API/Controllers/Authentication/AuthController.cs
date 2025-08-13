namespace API.Controllers.Authentication;

/// <summary>
/// Authentication and authorization endpoints
/// </summary>
[Route("api/v1/auth")]
[ApiController]
[Tags("Authentication")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;

    public AuthController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Authenticates user and returns JWT token
    /// </summary>
    /// <param name="loginModel">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    /// <remarks>
    /// Use this endpoint to obtain a JWT token for accessing protected endpoints.
    /// The token should be included in the Authorization header as "Bearer {token}".
    /// 
    /// Example:
    /// ```json
    /// {
    ///   "emailOrUsername": "admin@example.com",
    ///   "password": "your-password"
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Login successful, returns JWT token</response>
    /// <response code="400">Bad request - invalid input</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginModel loginModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userService.LoginAsync(loginModel.EmailOrUsername, loginModel.Password);

        if (user == null)
        {
            return Unauthorized(new { Success = false, Message = "Invalid credentials" });
        }

        var token = await _jwtService.GenerateTokensAsync(user);

        SetRefreshTokenCookie(token.RefreshToken);

        var returnModel = new
        {
            Success = true,
            Token = token.JwtToken,
            User = new UserDto
            {
                Username = user.Username,
                Email = user.Email,
                Role = user.UserRole
            }
        };

        return Ok(returnModel);
    }

    /// <summary>
    /// Logs out the user and invalidates refresh token
    /// </summary>
    /// <returns>Success response</returns>
    /// <response code="200">Logout successful</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout()
    {
        // Try to retrieve the refresh token from the cookie
        var refreshToken = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            // Invalidate the refresh token (if it exists)
            await _jwtService.RevokeRefreshTokenAsync(refreshToken);
        }

        // Clear the cookie regardless of whether the token is valid or not
        Response.Cookies.Append("refreshToken", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTime.UtcNow.AddDays(-1) // Set to past date
        });

        // Return success response
        return Ok(new { Success = true, Message = "Logout successful" });
    }

    /// <summary>
    /// Refreshes the access token using refresh token cookie
    /// </summary>
    /// <returns>New access token</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Bad request - no refresh token found</response>
    /// <response code="401">Unauthorized - invalid or expired refresh token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if(string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new { Success = false, Message = "No refresh token found." });
        }

        var tokens = await _jwtService.RefreshTokenAsync(refreshToken);

        SetRefreshTokenCookie(tokens.RefreshToken);

        return Ok(new
        {
            Success = true,
            AccessToken = tokens.JwtToken
        });
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Only send cookies over HTTPS
            Expires = DateTime.UtcNow.AddDays(7) // Token expiration
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}

