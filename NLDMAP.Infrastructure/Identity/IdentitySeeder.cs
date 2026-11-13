using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NLDMAP.Application.Security;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.Infrastructure.Identity;

//Este es el inicializador de la cuenta administrativa del sistema, que se encarga de crear la cuenta de administrador y asignarle el rol correspondiente
public sealed class IdentitySeeder(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    public async Task SeedSystemRolesAsync()
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        if (dbContext.Database.IsNpgsql())
            await dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(724910031)");
        await EnsureSystemRolesAsync();
        await transaction.CommitAsync();
    }

    public async Task SeedAdministratorAsync(string email, string password)
    {

        email = email.Trim();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        if (dbContext.Database.IsNpgsql()) await dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(724910031)");
        await EnsureSystemRolesAsync();
        // Una cuenta existente conserva su contraseña y sus datos.
        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, RoleNames.Administrator))
            {
                throw new InvalidOperationException("El  correo ya pertenece a una cuenta sin el rol administrativo. No se modificó esa cuenta. ");
            }
            await transaction.CommitAsync();
            return;
        }

        var user = new ApplicationUser { UserName = email, Email = email, DisplayName = "Administrador", IsActive = true };

        EnsureSucceeded(await userManager.CreateAsync(user, password));

        EnsureSucceeded(await userManager.AddToRoleAsync(user, RoleNames.Administrator));

        await transaction.CommitAsync();
    }

    private async Task EnsureSystemRolesAsync()
    {
        foreach (var name in RoleNames.SystemRoles)
            if (!await roleManager.RoleExistsAsync(name))
                EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole(name)));
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));

        throw new InvalidOperationException(errors);
    }
}

