using Application.Common.Interfaces;
using Application.Common.Models;

namespace Application.Authentication.Commands;

public record RefreshTokenCommand:IRequest<ResponseDto>
{
    public string? Token { get; init; } 
}
internal class HandleRefreshTokenCommand : IRequestHandler<RefreshTokenCommand, ResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public HandleRefreshTokenCommand(IIdentityService identityService, IUser currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }
    public async Task<ResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(_currentUser.Username))
        {
            throw new Common.Exceptions.ValidationException("Username cannot be empty");
        }
        var response = await _identityService.RefreshToken(_currentUser.Username, request.Token??"", cancellationToken);
        if (!response.IsSuccess)
            throw new Common.Exceptions.ValidationException(response.DisplayMessage);

        return response;
    }

    
}
