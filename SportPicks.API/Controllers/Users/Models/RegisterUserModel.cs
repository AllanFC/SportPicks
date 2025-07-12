using System.ComponentModel;

namespace API.Controllers.Users.Models;

public class RegisterUserModel
{
    [Required]
    [MinLength(2, ErrorMessage = "Username can not be shorter than 2 characters")]
    [MaxLength(25, ErrorMessage = "UserName can not be more than 25 characters")]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
#if DEBUG == false
    [MinLength(8)]
    [MaxLength(75)]
#endif
    public required string Password { get; set; }
}
