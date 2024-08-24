namespace Application.SampleEntities.Create;

public class CreateSampleRequestValidator: AbstractValidator<CreateSampleRequest>
{
    public CreateSampleRequestValidator()
    {
        RuleFor(m=>m.Name).NotEmpty();
        RuleFor(m=>m.Description).NotEmpty();
    }
}
