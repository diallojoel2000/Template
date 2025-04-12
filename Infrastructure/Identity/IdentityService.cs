using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDateTime _dateTime;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService, IDateTime dateTime, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _dateTime = dateTime;
        _httpContextAccessor = httpContextAccessor;

    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }
    public async Task<ResponseDto> LoginAsync(string username, string password, JwtDetail jwt)
    {
        try
        {
            var response = new ResponseDto();

            var user = await _userManager.Users.FirstOrDefaultAsync(m => m.UserName!.ToLower() == username.ToLower());
            if (user == null)
            {
                response.IsSuccess = false;
                response.DisplayMessage = "Invalid username or password";
                response.ErrorMessage = new List<string> { response.DisplayMessage };
                return response;
            }
            if (await _userManager.CheckPasswordAsync(user, password))
            {
                if (await _userManager.IsLockedOutAsync(user))
                {
                    response.IsSuccess = false;
                    response.DisplayMessage = "User is locked out";
                    response.ErrorMessage = new List<string> { response.DisplayMessage };
                    return response;
                }
                
                await _userManager.ResetAccessFailedCountAsync(user);
                var roles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>();
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                
                claims.Add(new Claim("Username", user.UserName));
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""));
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                

                var token = CreateToken(jwt, new ClaimsIdentity(claims));
                
                user.RefreshToken = GenerateRefreshToken();
                user.RefreshTokenExpiryTime = _dateTime.Now.AddDays(jwt.RefreshTokenValidityInDays);
                user.LastLogin = _dateTime.Now;

                await _userManager.UpdateAsync(user);
                
                response.Result = new TokenVm
                {
                    Token = token
                };
                _httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", user.RefreshToken);
                return response;
            }
            if (user.LockoutEnabled)
            {
                user.AccessFailedCount = user.AccessFailedCount + 1;
                if (user.AccessFailedCount == 5)
                {
                    await _userManager.SetLockoutEndDateAsync(user, _dateTime.Now.AddMinutes(5));
                    response.IsSuccess = false;
                    response.DisplayMessage = "You account has been locked";
                    response.ErrorMessage = new List<string> { response.DisplayMessage };
                    return response;
                }
                else
                {
                    await _userManager.UpdateAsync(user);
                    response.IsSuccess = false;
                    response.DisplayMessage = "Invalid username or password";//$"Your account will be locked after {5- user.AccessFailedCount} try(s)";
                    response.ErrorMessage = new List<string> { response.DisplayMessage };
                    return response;
                }
            }

            response.IsSuccess = false;
            response.DisplayMessage = "Invalid username or password";
            response.ErrorMessage = new List<string> { response.DisplayMessage };
            return response;
        }
        catch (Exception ex)
        {
            return new ResponseDto
            {
                IsSuccess = false,
                DisplayMessage = ex.Message,
                ErrorMessage = new List<string> { Convert.ToString(ex) }
            };
        }

    }
    public async Task<ResponseDto> RefreshToken(string username, string accessToken, JwtDetail jwt, CancellationToken cancellationToken)
    {
        var refreshToken = _httpContextAccessor.HttpContext?.Request?.Cookies["refreshToken"];
        var response = new ResponseDto();
        var principal = GetPrincipalFromExpiredToken(accessToken, jwt);
        if (principal == null)
        {
            response.IsSuccess = false;
            response.DisplayMessage = "Invalid access token or refresh token";
            response.ErrorMessage = new List<string> { response.DisplayMessage };
            return response;
        }
        var user = await _userManager.FindByNameAsync(username);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= _dateTime.Now)
        {
            response.IsSuccess = false;
            response.DisplayMessage = "Invalid access token or refresh token";
            response.ErrorMessage = new List<string> { response.DisplayMessage };
            return response;
        }

        var token = CreateToken(jwt, new ClaimsIdentity(principal.Claims.ToList()));
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.LastLogin = _dateTime.Now;
        await _userManager.UpdateAsync(user);
        
        response.Result = new TokenVm
        {
            Token = token
        };
        _httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", user.RefreshToken);
        return response;
    }
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token, JwtDetail jwt)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;

    }
    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }
    public string CreateToken(JwtDetail jwt, ClaimsIdentity claimsIdentity)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
                            {
                                Subject = claimsIdentity,
                                Expires = _dateTime.Now.AddMinutes(jwt.TokenValidityInMinutes),
                                Issuer = jwt.Issuer,
                                Audience = jwt.Audience,
                                SigningCredentials = new SigningCredentials
                                    (new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                                    SecurityAlgorithms.HmacSha512Signature)
                            };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}