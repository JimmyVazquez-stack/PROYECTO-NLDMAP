using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NLDMAP.Application.Accounts;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Infrastructure.Identity;

namespace NLDMAP.Server.Authentication;

[ApiController]
[Route("api/mobile/auth")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class MobileAuthController(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn,
    IOptionsMonitor<BearerTokenOptions> bearerOptions,
    ICitizenAccounts accounts,
    IAccountLifecycle lifecycle,
    IOptions<IdentityOptions> identity,
    ILogger<MobileAuthController> logger) : ControllerBase
{
    public const string Scheme = "Nldmap.Mobile";

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null || !RoleNames.IsCitizenAccount(await users.GetRolesAsync(user)))
            return InvalidLogin();

        var result = await signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded || await users.GetTwoFactorEnabledAsync(user))
            return InvalidLogin();

        return SignIn(await signIn.CreateUserPrincipalAsync(user), Scheme);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(CitizenRegistration request)
    {
        try
        {
            await accounts.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created);
        }
        catch (AdministrationException exception)
        {
            return Problem(title: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(RecoveryRequest request)
    {
        try
        {
            await lifecycle.RequestPasswordResetAsync(request.Email, AccountAudience.Citizen);
        }
        catch (Exception)
        {
            logger.LogError("Error procesando una solicitud de recuperación móvil.");
        }

        return NoContent();
    }

    [HttpPost("profile/email-code")]
    [Authorize(AuthenticationSchemes = Scheme)]
    public async Task<IActionResult> SendProfileEmailCode()
    {
        if (await ValidateSessionAsync(User) is null)
            return Unauthorized();

        try
        {
            await accounts.SendEmailConfirmationAsync(CurrentActor.FromPrincipal(User, identity.Value));
            return NoContent();
        }
        catch (AdministrationException exception)
        {
            return Problem(title: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost("profile")]
    [Authorize(AuthenticationSchemes = Scheme)]
    public async Task<ActionResult<CitizenProfile>> CompleteProfile(CitizenProfileInput request)
    {
        if (await ValidateSessionAsync(User) is null)
            return Unauthorized();

        try
        {
            return await accounts.CompleteProfileAsync(CurrentActor.FromPrincipal(User, identity.Value), request);
        }
        catch (AdministrationException exception)
        {
            return Problem(title: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
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

        var profile = await accounts.GetProfileAsync(CurrentActor.FromPrincipal(User, identity.Value));
        return new MobileProfile(user.Id, user.Email, user.DisplayName,
            profile?.Role, user.EmailConfirmed, profile?.CanCreateReports == true);
    }

    private async Task<ApplicationUser?> ValidateSessionAsync(ClaimsPrincipal principal)
    {
        var user = await signIn.ValidateSecurityStampAsync(principal);
        if (user is null || !await signIn.CanSignInAsync(user) ||
            await users.IsLockedOutAsync(user) || await users.GetTwoFactorEnabledAsync(user) ||
            !RoleNames.IsCitizenAccount(await users.GetRolesAsync(user)))
            return null;

        return user;
    }

    private ObjectResult InvalidLogin() => Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "No fue posible iniciar sesión.");

    public sealed record LoginRequest(
        [Required, EmailAddress, StringLength(256)] string Email,
        [Required, StringLength(1024)] string Password);

    public sealed record RecoveryRequest([Required, EmailAddress, StringLength(256)] string Email);

    public sealed record RefreshRequest([Required, StringLength(8192)] string RefreshToken);

    public sealed record MobileProfile(string Id, string? Email, string DisplayName,
        string? CitizenRole, bool EmailConfirmed, bool CanCreateReports);
}
