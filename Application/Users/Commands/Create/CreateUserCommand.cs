using Application.Common.Interfaces;
using Application.Common.Models;

namespace Application.Users;
public record CreateUserCommand:IRequest<ResponseDto>
{
    public string FullName { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
}
internal class CreateUserCommandHandler:IRequestHandler<CreateUserCommand, ResponseDto>
{
    private readonly IIdentityService _identityServices;
    public CreateUserCommandHandler(IIdentityService identityService)
    {
        _identityServices = identityService;
    }

    public async Task<ResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityServices.CreateUserAsync(request.FullName, request.UserName,request.Email,request.Password);
        if (!result.Result.Succeeded)
        {
            throw new Common.Exceptions.ValidationException(result.Result.Errors[0]);
        }

        return new ResponseDto
        {
            DisplayMessage = "User was created successfully"
        };
    }
}
