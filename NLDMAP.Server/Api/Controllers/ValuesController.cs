using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NLDMAP.Server.Api.Controllers
{
    [Route("api/saludo")]
    [ApiController]
    public sealed class ValuesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok( new { mensaje = "Hola, mundo!" });
        }
    }
}
    