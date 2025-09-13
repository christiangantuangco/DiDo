using System.ComponentModel.DataAnnotations;

namespace DiDo.Models;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Username or Email")]
    public required string UsernameOrEmail { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}