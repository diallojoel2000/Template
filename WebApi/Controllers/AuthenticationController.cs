using Application.Authentication.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class AuthenticationController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpPost]
    [Route("Logout")]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete("refreshToken");
        return Ok();
    }

    [HttpPost]
    [Route("RefreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        string refreshToken=string.Empty;
        Request.Cookies.TryGetValue("refreshToken", out refreshToken);
        return Ok();
    }
   
}
