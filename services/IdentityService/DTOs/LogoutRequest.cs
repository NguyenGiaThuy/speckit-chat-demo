using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs;

public class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
