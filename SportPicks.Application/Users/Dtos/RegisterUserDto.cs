using System.ComponentModel.DataAnnotations;

namespace Application.Users.Dtos;

public class RegisterUserDto
{
    [Required]
    [MaxLength(50)]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(6)]
    public required string Password { get; set; }
}
