using Application.Common.Models;

namespace Application.Common.Interfaces;
public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);
    Task<ResponseDto> LoginAsync(string username, string password, JwtDetail jwt);
    Task<ResponseDto> RefreshToken(string username, string accessToken, JwtDetail jwt, CancellationToken cancellationToken);
}