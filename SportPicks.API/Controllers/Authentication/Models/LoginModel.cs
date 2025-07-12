namespace API.Controllers.Authentication.Models;

public class LoginModel
{
    public required string EmailOrUsername { get; set; }
    public required string Password { get; set; }
}
