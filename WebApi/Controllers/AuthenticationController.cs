﻿using Application.Authentication.Commands;
using Application.Common.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class AuthenticationController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    [Route("Login")]
    public async Task<ActionResult<ResponseDto>> Login(LoginCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpPost]
    [Route("Logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("refreshToken");
        return Ok();
    }

    [HttpPost]
    [Route("RefreshToken")]
    public async Task<ActionResult<ResponseDto>> RefreshToken()
    {
        var token = await HttpContext.GetTokenAsync("access_token");
        var response = await Mediator.Send(new RefreshTokenCommand { Token= token });
        return Ok(response);
    }
   
}
