using Application.Users;
using Microsoft.AspNetCore.Mvc;


namespace WebApi.Controllers;
public class UsersController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers(GetPagedUsersQuery query)
    {
        var response = await Mediator.Send(query);
        return Ok(response);
    }
}
