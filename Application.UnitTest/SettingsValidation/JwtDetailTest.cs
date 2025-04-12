using Application.Common.Models;
using FluentAssertions;

namespace Application.UnitTest.SettingsValidation;

public class JwtDetailTest
{
    private const string ValidUrl = "https://localhost";
    private static JwtDetail ValidConfig()
    {
        return new JwtDetail
        {
            Issuer = ValidUrl,
            Audience = ValidUrl,
            Key = "This is just a test key and should not be used in production",
            TokenValidityInMinutes = 5,
            RefreshTokenValidityInDays = 1,
        };
    }
    public static TheoryData<JwtDetail> JwtDetailWithInvalidData()
    {
        var s = ValidConfig();

        return new TheoryData<JwtDetail>
        {
            s with { Issuer = "" },
            s with { Issuer = " " },
            s with { Issuer = "FakeContext" },
            s with { Audience = "" },
            s with { Audience = " " },
            s with { Audience = "FakeContext" },
            s with { Key = "" },
            s with { Key = " " },
            s with { TokenValidityInMinutes = 0 },
            s with { RefreshTokenValidityInDays = 0 },
        };
    }

    [Theory]
    [MemberData(nameof(JwtDetailWithInvalidData))]
    public void JwtDetailCannotBeEmpty(JwtDetail detail)
    {
        var validator = new JwtDetailValidator();
        var result = validator.Validate(detail);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void JwtDetailCanBeValid()
    {
        var detail = ValidConfig();
        var validator = new JwtDetailValidator();

        var result = validator.Validate(detail);
        result.IsValid.Should().BeTrue();
    }
}
