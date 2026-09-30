using Microsoft.AspNetCore.Identity;
using NLDMAP.Infrastructure.Identity;

namespace NLDMAP.API.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var roles = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var users = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var email = configuration["InitialAdmin:Email"]
            ?? throw new InvalidOperationException(
                "Falta configurar InitialAdmin:Email.");

        var password = configuration["InitialAdmin:Password"]
            ?? throw new InvalidOperationException(
                "Falta configurar InitialAdmin:Password.");

        foreach (var roleName in new[] { "Administrador", "Operador" })
        {
            if (!await roles.RoleExistsAsync(roleName))
            {
                Check(await roles.CreateAsync(
                    new IdentityRole(roleName)));
            }
        }

        var existingUser = await users.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            if (!await users.IsInRoleAsync(existingUser, "Administrador"))
            {
                throw new InvalidOperationException(
                    "Ese correo ya pertenece a una cuenta sin rol " +
                    "Administrador. Usa otro correo para la cuenta inicial.");
            }

            return;
        }

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email
        };

        Check(await users.CreateAsync(admin, password));
        Check(await users.AddToRoleAsync(admin, "Administrador"));
    }

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ",
                    result.Errors.Select(error => error.Description)));
        }
    }
}