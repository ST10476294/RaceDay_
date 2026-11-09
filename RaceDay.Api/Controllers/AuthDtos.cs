using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Controllers;
public class RegisterDto
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
    [Required, StringLength(72, MinimumLength = 8)] public string Password { get; set; } = "";
    [Required, RegularExpression("^(Organiser|Participant)$")]
    public string Role { get; set; } = "";
    /// <summary>Required only when Role is Organiser. Issued by the RaceDay administrator.</summary>
    [StringLength(100)] public string? OrganiserCode { get; set; }
}
public class LoginDto
{
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
    [Required, StringLength(72)] public string Password { get; set; } = "";
}
