using Application.Common.Models;
using Application.Users;
using Application.Users.Commands.ResetPassword;
using Microsoft.AspNetCore.Mvc;


namespace WebApi.Controllers;
public class UsersController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedList<UsersVm>>> GetUsers(int pageNumber, int pageSize, string search)
    {
        var query = new GetPagedUsersQuery { PageNumber = pageNumber, PageSize = pageSize, Search=search };
        var response = await Mediator.Send(query);
        return Ok(response);
    }
    [HttpPost]
    public async Task<ActionResult<ResponseDto>> CreateUser(CreateUserCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }
    [HttpPost]
    [Route("AdminResetPassword")]
    public async Task<ActionResult<ResponseDto>> AdminResetPassword(AdminPasswordResetCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }
}
