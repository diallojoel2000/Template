namespace Application.Authentication.Commands.Login;
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(v => v.Username)
           .NotEmpty();
        RuleFor(v => v.Password)
            .MinimumLength(6)
            .NotEmpty();
    }
}

