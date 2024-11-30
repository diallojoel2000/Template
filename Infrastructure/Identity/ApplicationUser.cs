using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Identity;
public class ApplicationUser : IdentityUser
{
    [MaxLength(150)]
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime? LastLogin { get; set; }
}