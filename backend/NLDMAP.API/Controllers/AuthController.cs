using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NLDMAP.Infrastructure.Identity;

namespace NLDMAP.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [Consumes("application/json")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Unauthorized(new
            {
                mensaje = "No fue posible iniciar sesión."
            });
        }

        var result = await signIn.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Unauthorized(new
            {
                mensaje = "No fue posible iniciar sesión."
            });
        }

        return Ok(new { mensaje = "Sesión iniciada." });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            email = User.FindFirstValue(ClaimTypes.Email),
            roles = User.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .ToArray()
        });
    }
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] object request)
    {
        await signIn.SignOutAsync();
        return Ok(new { mensaje = "Sesión cerrada." });
    }
    
    [Authorize(Roles = "Administrador")]
    [HttpPost("admin-check")]
    public IActionResult AdminCheck()
    {
        return Ok(new { mensaje = "Acceso de administrador confirmado." });
    }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}