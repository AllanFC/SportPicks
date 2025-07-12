namespace API.Controllers.Authentication.Models;

public class UserDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}
