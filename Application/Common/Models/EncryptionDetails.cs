namespace Application.Common.Models;

public record EncryptionDetails
{
    public string ClientKey { get; init; } = null!;
    public string ClientSalt { get; init; } = null!;
}

public class EncryptionDetailsValidator:AbstractValidator<EncryptionDetails>
{
    public EncryptionDetailsValidator()
    {
        RuleFor(m => m.ClientKey).NotEmpty();
        RuleFor(m => m.ClientSalt).NotEmpty();
    }
}