using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using NanoidDotNet;

namespace Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDateTime _dateTime;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtDetail _jwt;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService, IDateTime dateTime, 
        IHttpContextAccessor httpContextAccessor, IOptions<JwtDetail> jwt)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _dateTime = dateTime;
        _httpContextAccessor = httpContextAccessor;
        _jwt = jwt.Value;
        
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }
    public async Task<ResponseDto> LoginAsync(string username, string password)
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
                

                var token = CreateToken(new ClaimsIdentity(claims));
                
                user.RefreshToken = GenerateRefreshToken();
                user.RefreshTokenExpiryTime = _dateTime.Now.AddDays(_jwt.RefreshTokenValidityInDays);
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
    public async Task<ResponseDto> RefreshToken(string username, string accessToken, CancellationToken cancellationToken)
    {
        var refreshToken = _httpContextAccessor.HttpContext?.Request?.Cookies["refreshToken"];
        var response = new ResponseDto();
        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
        {
            response.IsSuccess = false;
            response.DisplayMessage = "Could not get claims from principal";
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

        var token = CreateToken(new ClaimsIdentity(principal.Claims.ToList()));
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = _dateTime.Now.AddDays(_jwt.RefreshTokenValidityInDays);
        user.LastLogin = _dateTime.Now;

        await _userManager.UpdateAsync(user);

        
        response.Result = new TokenVm
        {
            Token = token
        };
        _httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", user.RefreshToken);
        return response;
    }
    
    public async Task<(Result Result, string UserId)> CreateUserAsync(string fullName, string userName,string email, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            FullName = fullName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }
    public async Task<ResponseDto> AdminResetPassword(string id)
    {
        var response = new ResponseDto();
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            response.IsSuccess = false;
            response.DisplayMessage ="User does not exist";
            return response;
        }

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        var password = Nanoid.Generate(size: 8);
        var result = await _userManager.ResetPasswordAsync(user, code, password);
        if (result.Succeeded)
        {
            response.DisplayMessage = $"Password has been reset to {password}";
            return response;
        }
        response.IsSuccess = false;
        response.DisplayMessage = result!.Errors!.FirstOrDefault()!.Description;
        return response;
    }
    public async Task<ResponseDto> LockAccount(string id)
    {
        var response = new ResponseDto();
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            response.IsSuccess = false;
            response.DisplayMessage = "User does not exist";
            return response;
        }

        user.LockoutEnd = _dateTime.Now.AddYears(100);
        user.LockoutEnabled = true;
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            response.DisplayMessage = $"User is now disabled";
            return response;
        }
        response.IsSuccess = false;
        response.DisplayMessage = result!.Errors!.FirstOrDefault()!.Description;
        return response;
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

    public async Task<ResponseDto> GetUsers(int pageNumber, int pageSize, string search)
    {
        //var where = BuildDynamicWhereClause(search);
        var where = BuildDynamicWhereClause<ApplicationUser>(search, "FullName","Email", "UserName");
        var result = await _userManager.Users.Where(where).OrderBy(m => m.Id)
            .PaginatedListAsync(pageNumber, pageSize);
        return new ResponseDto
        {
            Result = JsonConvert.SerializeObject(result),
        };
    }

    #region Private Methods
    private Expression<Func<ApplicationUser, bool>> BuildDynamicWhereClause(string searchValue)
    {
        var predicate = PredicateBuilder.New<ApplicationUser>(true); 
        if (string.IsNullOrWhiteSpace(searchValue) == false)
        {
            var searchTerms = searchValue.Split(' ').ToList().ConvertAll(x => x.ToLower());

            predicate = predicate.Or(s => searchTerms.Any(srch => s.FullName.ToLower().Contains(srch)));
            predicate = predicate.Or(s => searchTerms.Any(srch => s.Email!.ToLower().Contains(srch)));
            predicate = predicate.Or(s => searchTerms.Any(srch => s.UserName!.ToLower().Contains(srch)));
        }
        return predicate;
    }
    public Expression<Func<T, bool>> BuildDynamicWhereClause<T>(string searchValue, params string[] propertyNames)
    {
        var predicate = PredicateBuilder.New<T>(true);

        if (!string.IsNullOrWhiteSpace(searchValue) && propertyNames != null && propertyNames.Length > 0)
        {
            var searchTerms = searchValue.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(x => x.ToLower());
            var parameter = Expression.Parameter(typeof(T), "entity");

            foreach (var propertyName in propertyNames)
            {
                var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null && (propertyInfo.PropertyType == typeof(string) || propertyInfo.PropertyType.IsValueType || (Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null && Nullable.GetUnderlyingType(propertyInfo.PropertyType) == typeof(string))))
                {
                    Expression propertyExpression = Expression.Property(parameter, propertyInfo);

                    // Convert to string for consistent searching
                    if (propertyInfo.PropertyType != typeof(string))
                    {
                        propertyExpression = Expression.Call(propertyExpression, "ToString", Type.EmptyTypes);
                    }

                    // Make it lowercase for case-insensitive search
                    var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                    Expression lowerCaseProperty = Expression.Call(propertyExpression, toLowerMethod);

                    foreach (var searchTerm in searchTerms)
                    {
                        var searchTermExpression = Expression.Constant(searchTerm);
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        var containsCall = Expression.Call(lowerCaseProperty, containsMethod, searchTermExpression);

                        predicate = predicate.Or(Expression.Lambda<Func<T, bool>>(containsCall, parameter));
                    }
                }
                else
                {
                    // Handle cases where the property doesn't exist or is not a string (or convertible to string)
                    // You might want to log a warning or throw an exception depending on your needs.
                    Console.WriteLine($"Warning: Property '{propertyName}' not found or not a string/convertible to string on type '{typeof(T).Name}'.");
                }
            }
        }

        return predicate;
    }
    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }
    private string CreateToken(ClaimsIdentity claimsIdentity)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = _dateTime.Now.AddMinutes(_jwt.TokenValidityInMinutes),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            SigningCredentials = new SigningCredentials
                                    (new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
                                    SecurityAlgorithms.HmacSha512Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = _jwt.Issuer,
            ValidAudience = _jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
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

    #endregion
}