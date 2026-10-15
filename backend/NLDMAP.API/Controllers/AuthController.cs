using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLDMAP.Application.Users.Registration;
using NLDMAP.Infrastructure.Identity;
using NLDMAP.Application.Security;
using NLDMAP.API.Authorization;
using Npgsql;

namespace NLDMAP.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn,
    PermissionAccessService permissionAccess) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    [Consumes("application/json")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterAccountRequest request)
    {
        // ApiController validates required fields and email format before this action runs.
        var email = request.Email.Trim();

        if (await users.FindByEmailAsync(email) is not null)
        {
            return EmailAlreadyRegistered();
        }

        // Only map the fields accepted by public registration.
        var user = new ApplicationUser
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            UserName = email,
            Email = email
        };

        IdentityResult result;

        try
        {
            // Identity validates the password, hashes it, and persists the account.
            // Do not trim or otherwise modify the password.
            result = await users.CreateAsync(user, request.Password);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateAccount(exception))
        {
            // Another request may insert the same account after the initial lookup.
            return EmailAlreadyRegistered();
        }

        if (!result.Succeeded)
        {
            if (result.Errors.Any(error =>
                error.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                return EmailAlreadyRegistered();
            }

            foreach (var error in result.Errors)
            {
                var field = error.Code.StartsWith(
                    "Password", StringComparison.Ordinal)
                    ? nameof(RegisterAccountRequest.Password)
                    : error.Code is "InvalidEmail" or "InvalidUserName"
                        ? nameof(RegisterAccountRequest.Email)
                        : string.Empty;

                ModelState.AddModelError(
                    field, GetRegistrationErrorMessage(error));
            }

            return ValidationProblem(ModelState);
        }

        // Public registration does not assign roles or start a session.
        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "La cuenta se creó correctamente.",
            user = new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email
            }
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [Consumes("application/json")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email);

        if (user is null || !user.IsActive)
        {
            return Unauthorized(new
            {
                message = "No fue posible iniciar sesión."
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
                message = "No fue posible iniciar sesión."
            });
        }

        return Ok(new { message = "Sesión iniciada." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await users.GetUserAsync(User);

        if (user is null || !user.IsActive)
        {
            return Unauthorized();
        }

        var assignedRoles = await users.GetRolesAsync(user);
        var roles = assignedRoles.Where(RoleNames.IsAssignable).ToArray();
        var retiredRoles = assignedRoles
            .Where(role => !RoleNames.IsAssignable(role))
            .ToArray();
        var permissions = await permissionAccess.GetPermissionsAsync(
            user.Id, HttpContext.RequestAborted);

        return Ok(new
        {
            id = user.Id,
            firstName = user.FirstName,
            lastName = user.LastName,
            email = user.Email,
            roles,
            retiredRoles,
            permissions
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] object request)
    {
        await signIn.SignOutAsync();
        return Ok(new { message = "Sesión cerrada." });
    }

    [Authorize(Policy = PermissionNames.UsersRead)]
    [HttpPost("admin-check")]
    public IActionResult AdminCheck()
    {
        return Ok(new
        {
            message = "Permiso de consulta de usuarios confirmado.",
            permission = PermissionNames.UsersRead
        });
    }

    private IActionResult EmailAlreadyRegistered()
    {
        ModelState.AddModelError(
            nameof(RegisterAccountRequest.Email),
            "El correo electrónico ya está registrado.");

        return ValidationProblem(
            statusCode: StatusCodes.Status409Conflict,
            title: "No se pudo crear la cuenta.",
            modelStateDictionary: ModelState);
    }

    private static bool IsDuplicateAccount(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgres
            && postgres.SqlState == PostgresErrorCodes.UniqueViolation
            && postgres.ConstraintName is "EmailIndex" or "UserNameIndex";
    }

    private string GetRegistrationErrorMessage(IdentityError error)
    {
        return error.Code switch
        {
            "PasswordTooShort" =>
                $"La contraseña debe tener al menos {users.Options.Password.RequiredLength} caracteres.",
            "PasswordRequiresNonAlphanumeric" =>
                "La contraseña debe incluir al menos un símbolo.",
            "PasswordRequiresDigit" =>
                "La contraseña debe incluir al menos un número.",
            "PasswordRequiresLower" =>
                "La contraseña debe incluir al menos una letra minúscula.",
            "PasswordRequiresUpper" =>
                "La contraseña debe incluir al menos una letra mayúscula.",
            "PasswordRequiresUniqueChars" =>
                $"La contraseña debe incluir al menos {users.Options.Password.RequiredUniqueChars} caracteres diferentes.",
            "InvalidEmail" =>
                "El correo electrónico tiene un formato inválido.",
            "InvalidUserName" =>
                "El correo electrónico no es compatible con el identificador de cuenta.",
            _ =>
                "No se pudo crear la cuenta con los datos proporcionados."
        };
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
