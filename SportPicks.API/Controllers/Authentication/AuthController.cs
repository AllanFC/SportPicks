namespace API.Controllers.Authentication;

[Route("api/auth")]
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
        var user = await _userService.LoginAsync(loginModel.Email, loginModel.Password);

        if (user == null)
        {
            return Unauthorized();
        }

        var token = _jwtService.GenerateToken(user);
        return Ok(new { Token = token });
    }
}

