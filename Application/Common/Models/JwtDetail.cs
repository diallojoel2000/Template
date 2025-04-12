using System.Text;

namespace Application.Common.Models;
public record JwtDetail
{
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string Key { get; set; } = null!;
    public byte[]? KeyHash => Encoding.UTF8.GetBytes(Key);
    public int TokenValidityInMinutes { get; set; }
    public int RefreshTokenValidityInDays { get; set; }
}

public class JwtDetailValidator : AbstractValidator<JwtDetail>
{
    public JwtDetailValidator()
    {
        RuleFor(m => m.Issuer).NotEmpty().Must(IsAbsoluteUrl).WithMessage("Issuer must have an absolute Url");
        RuleFor(m=>m.Audience).NotEmpty().Must(IsAbsoluteUrl).WithMessage("Audience must have an absolute Url");
        RuleFor(m => m.Key).NotEmpty();
        RuleFor(m => m.TokenValidityInMinutes).GreaterThan(0);
        RuleFor(m => m.RefreshTokenValidityInDays).GreaterThan(0);

    }

    private static bool IsAbsoluteUrl(string? url) => Uri.IsWellFormedUriString(url, UriKind.Absolute);
}