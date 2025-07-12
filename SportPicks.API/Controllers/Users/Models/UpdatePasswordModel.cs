namespace API.Controllers.Users.Models;

public class UpdatePasswordModel
{
    [Required]
    public required string OldPassword { get; set; }

    [Required]
#if DEBUG == false
    [MinLength(8)]
    [MaxLength(75)]
#endif
    public required string NewPassword { get; set; }

    [Required]
#if DEBUG == false
    [MinLength(8)]
    [MaxLength(75)]
#endif
    public required string ConfirmPassword { get; set; }
}
