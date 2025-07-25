using Application.Common.Interfaces;
using Application.Common.Models;

namespace Application.Users.Commands.ResetPassword;
public record AdminPasswordResetCommand :IRequest<ResponseDto>
{
    public string UserId { get; init; } = null!;
}
internal class AdminPasswordResetCommandHandler : IRequestHandler<AdminPasswordResetCommand, ResponseDto>
{
    private readonly IIdentityService _identityService;
    public AdminPasswordResetCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task<ResponseDto> Handle(AdminPasswordResetCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.AdminResetPassword(request.UserId);
    }
}