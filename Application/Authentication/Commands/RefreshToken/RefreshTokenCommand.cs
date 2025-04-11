using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Microsoft.Extensions.Options;


namespace Application.Authentication.Commands;

public record RefreshTokenCommand:IRequest<ResponseDto>
{
    public string? Token { get; init; } 
}
internal class HandleRefreshTokenCommand : IRequestHandler<RefreshTokenCommand, ResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly JwtDetail _jwtDetail;
    private readonly IUser _currentUser;

    public HandleRefreshTokenCommand(IIdentityService identityService, IOptions<JwtDetail> jwtDetail, IUser currentUser)
    {
        _identityService = identityService;
        _jwtDetail = jwtDetail.Value;
        _currentUser = currentUser;
    }
    public async Task<ResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(_currentUser.Username))
        {
            throw new Common.Exceptions.ValidationException("Username cannot be empty");
        }
        var response = await _identityService.RefreshToken(_currentUser.Username, request.Token??"",_jwtDetail, cancellationToken);
        if (!response.IsSuccess)
            throw new Common.Exceptions.ValidationException(response.DisplayMessage);

        return response;
    }

    
}
