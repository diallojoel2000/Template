using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Application.Common.Exceptions;

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
        private JwtDetail _jwtDetail;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDateTime _dateTime;
        private readonly IApplicationDbContext _context;
        public LoginCommandHandler(IApplicationDbContext context, IDateTime dateTime,IHttpContextAccessor httpContextAccessor,IIdentityService identityService,IOptions<JwtDetail> jwt, IEncryptionService encryptionService)
        {
            _identityService = identityService;
            _encryptionService = encryptionService;
            _jwtDetail = jwt.Value;
            _httpContextAccessor = httpContextAccessor;
            _dateTime = dateTime;
            _context = context;
        }

        public async Task<ResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var response = await _identityService.LoginAsync(request.Username, request.Password, _jwtDetail);
            //var response = await _identityService.LoginAsync(_encryptionService.DecryptAes(request.Username), _encryptionService.DecryptAes(request.Password), _jwtDetail);
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
                IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                DateAdded = _dateTime.Now,
                Action = action,
            };
            _context.LoginLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
