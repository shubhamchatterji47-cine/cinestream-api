using System.ComponentModel.DataAnnotations;

namespace CineStream.DTOs
{
  public class UserDto
  {
    [Required]
    public string FullName { get; set; }

    [Required]
    [EmailAddress] // ✅ email format validation
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string PasswordHash { get; set; }
  }
}
