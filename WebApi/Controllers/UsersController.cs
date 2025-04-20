using Application.Common.Models;
using Application.Users;
using Microsoft.AspNetCore.Mvc;


namespace WebApi.Controllers;
public class UsersController : ApiControllerBase
{
    [HttpGet]
    public async Task<PaginatedList<UsersVm>> GetUsers(int pageNumber, int pageSize, string search)
    {
        var query = new GetPagedUsersQuery { PageNumber = pageNumber, PageSize = pageSize, Search=search };
        var response = await Mediator.Send(query);
        return response;
    }
    [HttpPost]
    public async Task<ResponseDto> CreateUser(CreateUserCommand command)
    {
        var response = await Mediator.Send(command);
        return response;
    }
}
