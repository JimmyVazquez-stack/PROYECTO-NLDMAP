using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NLDMAP.API.Controllers;

[ApiController]
[Route("api/auth/csrf")]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CsrfController(IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            headerName = tokens.HeaderName,
            requestToken = tokens.RequestToken
        });
    }
}
