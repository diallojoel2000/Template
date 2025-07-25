
using Application.Roles;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
public class RolesController : ApiControllerBase
{
    [HttpGet]
    [Route("GetRoleMatrix")]
    public async Task<ActionResult<RoleMatrix>> GetRoleMatrix()
    {
        var response = await Mediator.Send(new GetRoleMatrixQuery());
        return Ok(response);
    }
}
