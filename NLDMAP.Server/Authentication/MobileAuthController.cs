using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NLDMAP.Infrastructure.Identity;

namespace NLDMAP.Server.Authentication;

[ApiController]
[Route("api/mobile/auth")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class MobileAuthController(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn,
    IOptionsMonitor<BearerTokenOptions> bearerOptions) : ControllerBase
{
    public const string Scheme = "Nldmap.Mobile";

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return InvalidLogin();

        var result = await signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded || await users.GetTwoFactorEnabledAsync(user))
            return InvalidLogin();

        return SignIn(await signIn.CreateUserPrincipalAsync(user), Scheme);
    }
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var email = request.Email.Trim();
        var displayName = request.DisplayName.Trim();

        if (displayName.Length == 0)
        {
            return Problem(
                title: "Escribe tu nombre.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            IsActive = true
        };

        var result = await users.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return Problem(
                title: "No se pudo crear la cuenta.",
                detail: "Revisa el correo y usa una contraseña de al menos 12 caracteres que cumpla los requisitos.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var options = bearerOptions.Get(Scheme);
        var ticket = options.RefreshTokenProtector.Unprotect(request.RefreshToken);
        if (ticket?.Properties.ExpiresUtc is not { } expires ||
            expires <= TimeProvider.System.GetUtcNow())
            return Unauthorized();

        var user = await ValidateSessionAsync(ticket.Principal);
        if (user is null)
            return Unauthorized();

        return SignIn(await signIn.CreateUserPrincipalAsync(user), Scheme);
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = Scheme)]
    public async Task<ActionResult<MobileProfile>> Me()
    {
        var user = await ValidateSessionAsync(User);
        if (user is null)
            return Unauthorized();

        return new MobileProfile(user.Id, user.Email, user.DisplayName);
    }

    private async Task<ApplicationUser?> ValidateSessionAsync(ClaimsPrincipal principal)
    {
        var user = await signIn.ValidateSecurityStampAsync(principal);
        if (user is null || !await signIn.CanSignInAsync(user) ||
            await users.IsLockedOutAsync(user) || await users.GetTwoFactorEnabledAsync(user))
            return null;

        return user;
    }

    private ObjectResult InvalidLogin() => Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "No fue posible iniciar sesión.");

    public sealed record LoginRequest(
        [Required, EmailAddress, StringLength(256)] string Email,
        [Required, StringLength(1024)] string Password);
    
    public sealed record RegisterRequest(
        [Required, StringLength(120)] string DisplayName,
        [Required, EmailAddress, StringLength(256)] string Email,
        [Required, StringLength(1024)] string Password);

    public sealed record RefreshRequest([Required, StringLength(8192)] string RefreshToken);

    public sealed record MobileProfile(string Id, string? Email, string DisplayName);
}
