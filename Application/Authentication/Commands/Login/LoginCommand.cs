using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Authentication.Commands
{
    public record LoginCommand:IRequest<ResponseDto>
    {
        public string Username { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ResponseDto>
    {
        private readonly IIdentityService _identityService;
        private IEncryptionService _encryptionService;
        private readonly IDateTime _dateTime;
        private readonly IApplicationDbContext _context;
        private readonly IUser _currentUser;
        public LoginCommandHandler(IApplicationDbContext context, IDateTime dateTime,IIdentityService identityService, IEncryptionService encryptionService,IUser currentUser)
        {
            _identityService = identityService;
            _encryptionService = encryptionService;
            _dateTime = dateTime;
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<ResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            //var response = await _identityService.LoginAsync(request.Username, request.Password, _jwtDetail);
            var response = await _identityService.LoginAsync(_encryptionService.DecryptAes(request.Username), _encryptionService.DecryptAes(request.Password));
            if(!response.IsSuccess)
            {
                throw new Common.Exceptions.ValidationException(response.DisplayMessage);
            }
            await LogAuthentication(request.Username, "Login", cancellationToken);
            return response;
        }
        private async Task LogAuthentication(string username, string action, CancellationToken cancellationToken)
        {
            var log = new LoginLog
            {
                Username = username,
                IpAddress = _currentUser.IpAddress??"NA",
                DateAdded = _dateTime.Now,
                Action = action,
            };
            _context.LoginLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
