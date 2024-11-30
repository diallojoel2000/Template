using Application.SampleEntities.Create;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    public class SampleDatasController : ApiControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateSample(CreateSampleRequest request)
        {
            var response = await Mediator.Send(request);
            return Ok(response);
        }
    }
}
