using System.ComponentModel.DataAnnotations;

namespace DiDo.Models;

public class RegisterViewModel
{
    [Required]
    public required string FirstName { get; set; }
    [Required]
    public required string LastName { get; set; }
    [Display(Name = "Username (Optional)")]
    public string? Username { get; set; }
    [Required]
    public required string Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}