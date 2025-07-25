using Application.Common.Models;

namespace Application.Common.Interfaces;
public interface IIdentityService
{
    Task<ResponseDto> GetUsers(int pageNumber, int pageSize, string search);
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string fullName ,string userName,string email, string password);

    Task<Result> DeleteUserAsync(string userId);
    Task<ResponseDto> LoginAsync(string username, string password);
    Task<ResponseDto> RefreshToken(string username, string accessToken, CancellationToken cancellationToken);
    Task<ResponseDto> AdminResetPassword(string id);
}