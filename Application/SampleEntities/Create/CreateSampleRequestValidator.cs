using Application.Common.Interfaces;

namespace Application.SampleEntities.Create;

public class CreateSampleRequestValidator: AbstractValidator<CreateSampleRequest>
{
    private readonly IUser _user;
    public CreateSampleRequestValidator(IUser user)
    {
        _user = user;
        RuleFor(m=>m.Name).NotEmpty().Must(HasUserId).WithMessage("UserID is null");
        RuleFor(m=>m.Description).NotEmpty();
    }
    public bool HasUserId(string name)
    {
        return _user.Id != null;
    }
}
