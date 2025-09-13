using System.ComponentModel.DataAnnotations;

namespace DiDo.Models;

public class User
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? UserName { get; set; }
    public required string Email { get; set; }
    public string? PasswordHash { get; set; }
    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; }
    public Entry[]? Entries { get; set; }
}