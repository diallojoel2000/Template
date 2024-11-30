using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;
public class LoginLog
{
    public long Id { get; set; }
    [MaxLength(50)]
    public required string Username { get; set; }
    [MaxLength(15)]
    public required string IpAddress { get; set; }
    [MaxLength(15)]
    public required string Action { get; set; }
    public DateTime DateAdded { get; set; }
}