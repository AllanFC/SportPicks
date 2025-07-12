namespace API.Controllers.Authentication;

[Route("api/v1/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;

    public AuthController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginModel loginModel)
    {
        var user = await _userService.LoginAsync(loginModel.EmailOrUsername, loginModel.Password);

        if (user == null)
        {
            return Unauthorized();
        }

        var token = await _jwtService.GenerateTokensAsync(user);

        SetRefreshTokenCookie(token.RefreshToken);

        var returnModel = new
        {
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

    [HttpPost("logout")]
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
        return Ok();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if(string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest("No refresh token found.");
        }

        try
        {
            var tokens = await _jwtService.RefreshTokenAsync(refreshToken);

            SetRefreshTokenCookie(tokens.RefreshToken);

            return Ok(new
            {
                AccessToken = tokens.JwtToken
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
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

