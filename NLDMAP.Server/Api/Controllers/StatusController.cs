using Microsoft.AspNetCore.Mvc;


namespace NLDMAP.Server.Api.Controllers
{
    [Route("api/status")]
    [ApiController]
    public sealed class StatusController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { application= "NLDMAP", status = "Corriendo" });
        }
    }
}
