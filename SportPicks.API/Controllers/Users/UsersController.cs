namespace SportPicks.API.Controllers.Users;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateUser([FromBody] RegisterUserModel dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = await _userService.RegisterUserAsync(dto.Username, dto.Email, dto.Password);
            return Ok(new { UserId = userId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{userId}/password")]
    public async Task<IActionResult> UpdatePassword(string email, [FromBody] UpdatePasswordModel dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.OldPassword == dto.NewPassword)
            return BadRequest(new { Error = "New password must be different from the old password." });

        if (dto.NewPassword != dto.ConfirmPassword)
            return BadRequest(new { Error = "Passwords do not match." });

        try
        {
            await _userService.UpdateUserPasswordAsync(email, dto.OldPassword, dto.NewPassword);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
