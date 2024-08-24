using Application.SampleEntities.Create;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    public class SampleDatasController : ApiControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateSample(CreateSampleRequest request)
        {
            try
            {
                await Mediator.Send(request);
              
            }
            catch(Exception ex) 
            { 
                var message = ex.Message;
            }
            return Ok();
        }
    }
}
