using Application.SampleEntities.Create;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    public class SampleDatasController : ApiControllerBase
    {
        [HttpPost]
        public IActionResult CreateSample(CreateSampleRequest request)
        {
            try
            {
                Mediator.Send(request);
              
            }
            catch(Exception ex) 
            { 
                var message = ex.Message;
            }
            return Ok();
        }
    }
}
