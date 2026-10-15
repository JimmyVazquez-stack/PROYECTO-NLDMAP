using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NLDMAP.Application.Security;
using NLDMAP.Infrastructure.Identity;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.API.Identity;

public static class IdentitySeeder
{
    private const string LegacyAdministratorRole = "Administrador";
    private const string LegacyOperatorRole = "Operador";

    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var database = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        var roles = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(IdentitySeeder));

        var email = configuration["InitialAdmin:Email"]?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException(
                "Falta configurar InitialAdmin:Email.");
        }

        var resetPassword =
            configuration.GetValue<bool>("ResetInitialAdminPassword");
        var environment = scope.ServiceProvider
            .GetRequiredService<IHostEnvironment>();

        if (resetPassword && !environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "ResetInitialAdminPassword solo está permitido en Development.");
        }

        var passwordWasReset = false;

        await using var transaction =
            await database.Database.BeginTransactionAsync();

        await MigrateAdministratorRoleAsync(roles, users);

        foreach (var roleName in RoleNames.All)
        {
            var role = await roles.FindByNameAsync(roleName);
            var isNewRole = role is null;

            if (role is null)
            {
                role = new IdentityRole(roleName);
                Check(await roles.CreateAsync(role));
            }
            else if (!string.Equals(role.Name, roleName, StringComparison.Ordinal))
            {
                role.Name = roleName;
                Check(await roles.UpdateAsync(role));
            }

            var allowedPermissions =
                RolePermissionDefaults.GetAllowedPermissions(roleName);
            var existingClaims = await roles.GetClaimsAsync(role);

            // Remove permission claims outside this stable role's approved boundary.
            foreach (var claim in existingClaims.Where(claim =>
                claim.Type == PermissionNames.ClaimType
                && !allowedPermissions.Contains(claim.Value, StringComparer.Ordinal)))
            {
                Check(await roles.RemoveClaimAsync(role, claim));
            }

            // Initialize new functional roles. Preserve deliberate reductions on later runs.
            // The bootstrap system-administrator role always retains its technical baseline.
            if (isNewRole || roleName == RoleNames.SystemAdministrator)
            {
                var retainedClaims = await roles.GetClaimsAsync(role);

                foreach (var permission in allowedPermissions)
                {
                    if (!retainedClaims.Any(claim =>
                        claim.Type == PermissionNames.ClaimType
                        && claim.Value == permission))
                    {
                        Check(await roles.AddClaimAsync(
                            role,
                            new Claim(PermissionNames.ClaimType, permission)));
                    }
                }
            }
        }

        await RetireOperatorRoleAsync(database, roles, logger);

        var admin = await users.FindByEmailAsync(email);

        if (admin is not null)
        {
            if (!await users.IsInRoleAsync(
                admin, RoleNames.SystemAdministrator))
            {
                throw new InvalidOperationException(
                    "Ese correo pertenece a una cuenta sin el rol " +
                    "Administrador del sistema. Usa otro correo para la cuenta inicial.");
            }

            if (resetPassword)
            {
                await ResetAdministratorPasswordAsync(admin, users, configuration);
                passwordWasReset = true;
            }
        }
        else
        {
            var password = configuration["InitialAdmin:Password"]
                ?? throw new InvalidOperationException(
                    "Falta configurar InitialAdmin:Password.");

            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = configuration["InitialAdmin:FirstName"] ?? "System",
                LastName = configuration["InitialAdmin:LastName"] ?? "Administrator"
            };

            Check(await users.CreateAsync(admin, password));
            Check(await users.AddToRoleAsync(
                admin, RoleNames.SystemAdministrator));
        }

        await transaction.CommitAsync();

        if (passwordWasReset)
        {
            logger.LogInformation(
                "Contraseña del administrador inicial restablecida correctamente.");
        }
    }

    private static async Task ResetAdministratorPasswordAsync(
        ApplicationUser admin,
        UserManager<ApplicationUser> users,
        IConfiguration configuration)
    {
        var password = configuration["InitialAdmin:Password"];

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Falta configurar InitialAdmin:Password para restablecer la contraseña.");
        }

        // Identity validates the new password and updates the password hash and security stamp.
        var token = await users.GeneratePasswordResetTokenAsync(admin);
        Check(await users.ResetPasswordAsync(admin, token, password));

        if (users.SupportsUserLockout)
        {
            Check(await users.ResetAccessFailedCountAsync(admin));
            Check(await users.SetLockoutEndDateAsync(admin, null));
        }
    }

    private static async Task MigrateAdministratorRoleAsync(
        RoleManager<IdentityRole> roles,
        UserManager<ApplicationUser> users)
    {
        var legacy = await roles.FindByNameAsync(LegacyAdministratorRole);

        if (legacy is null)
        {
            return;
        }

        var current = await roles.FindByNameAsync(RoleNames.SystemAdministrator);

        if (current is null)
        {
            // Preserve the role ID and existing account memberships.
            legacy.Name = RoleNames.SystemAdministrator;
            Check(await roles.UpdateAsync(legacy));
            return;
        }

        // If both names already exist, move memberships to the canonical technical role.
        var members = await users.GetUsersInRoleAsync(LegacyAdministratorRole);

        foreach (var member in members)
        {
            if (!await users.IsInRoleAsync(member, RoleNames.SystemAdministrator))
            {
                Check(await users.AddToRoleAsync(
                    member, RoleNames.SystemAdministrator));
            }

            Check(await users.RemoveFromRoleAsync(member, LegacyAdministratorRole));
        }

        Check(await roles.DeleteAsync(legacy));
    }

    private static async Task RetireOperatorRoleAsync(
        ApplicationDbContext database,
        RoleManager<IdentityRole> roles,
        ILogger logger)
    {
        var legacy = await roles.FindByNameAsync(LegacyOperatorRole);

        if (legacy is null)
        {
            return;
        }

        foreach (var claim in (await roles.GetClaimsAsync(legacy))
            .Where(claim => claim.Type == PermissionNames.ClaimType))
        {
            Check(await roles.RemoveClaimAsync(legacy, claim));
        }

        var membershipCount = await database.UserRoles
            .CountAsync(membership => membership.RoleId == legacy.Id);

        if (membershipCount == 0)
        {
            Check(await roles.DeleteAsync(legacy));
            return;
        }

        // Retain memberships for review, without assigning a functional replacement.
        logger.LogWarning(
            "El rol legado Operador conserva {Count} asignaciones pendientes de revisión. " +
            "Sus permisos están retirados del catálogo activo.",
            membershipCount);
    }

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }
}
