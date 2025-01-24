namespace API.Controllers.Authentication.Models;

public class LoginModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
