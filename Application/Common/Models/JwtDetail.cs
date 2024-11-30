using System.Text;

namespace Application.Common.Models;
public class JwtDetail
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public byte[]? KeyHash => Encoding.UTF8.GetBytes(Key);
    public int TokenValidityInMinutes { get; set; }
    public int RefreshTokenValidityInDays { get; set; }
}
