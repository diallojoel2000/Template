namespace Application.Users;
public class CreateUserValidator:AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(m => m.FullName).NotEmpty().MaximumLength(150);
        RuleFor(m => m.UserName).NotEmpty().MaximumLength(50);
        RuleFor(m => m.Email).NotEmpty().EmailAddress().MaximumLength(50);
        RuleFor(m => m.Password).NotEmpty().MinimumLength(6).MaximumLength(50);
    }
}
