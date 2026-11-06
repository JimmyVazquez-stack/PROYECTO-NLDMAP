using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using Microsoft.EntityFrameworkCore;
using NLDMAP.Infrastructure;
using NLDMAP.Infrastructure.Identity;
using NLDMAP.Infrastructure.Persistence;
using NLDMAP.Server.Components;
using Microsoft.AspNetCore.Components.Authorization;
using NLDMAP.Server.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using MudBlazor.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Se registran los servicios para trabajar con los controladores
builder.Services.AddControllers();

// Se registran los servicios de identity y persistencia de datos
builder.Services.AddInfrastructure(builder.Configuration);

// El servicio de autenticación se configura para usar cookies y el esquema de identidad predeterminado
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();

// Se registran los servicios de para la comprobación de autorización y se habilita la política de autorización predeterminada
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in PermissionNames.Catalog.Keys.Append(PermissionNames.Access))
        options.AddPolicy(permission, policy => policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(permission)));
});
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<ICurrentActor, CurrentActor>();
builder.Services.AddScoped<AdministrationService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        HttpMethods.IsPost(context.Request.Method) && context.Request.Path.StartsWithSegments("/Account")
            ? RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "local", _ =>
                new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })
            : RateLimitPartition.GetNoLimiter("other"));
});

// Se registran los servicios de autenticación y autorización para Blazor Server
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromMinutes(1));
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Events.OnValidatePrincipal = async context =>
    {
        await SecurityStampValidator.ValidatePrincipalAsync(context);
        if (context.Principal?.Identity?.IsAuthenticated != true) return;
        var users = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.GetUserAsync(context.Principal);
        if (user is null || !user.IsActive) context.RejectPrincipal();
    };
});

// Se registran los servicios de MudBlazor
builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Se configura el middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Configuration.GetValue<bool>("MigrateDatabase"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    app.Logger.LogInformation("Migraciones aplicadas.");
    if (!app.Configuration.GetValue<bool>("SeedIdentity") && !app.Configuration.GetValue<bool>("SeedRoles")) return;
}

if (app.Configuration.GetValue<bool>("SeedRoles"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedSystemRolesAsync();
    app.Logger.LogInformation("Roles del sistema inicializados.");
    if (!app.Configuration.GetValue<bool>("SeedIdentity")) return;
}

if (app.Configuration.GetValue<bool>("SeedIdentity"))
{
    if (!app.Environment.IsDevelopment())
    {
        throw new InvalidOperationException("La inicialización de Identity solo esta habilitada en el entorno de desarrollo");
    }
    var email = app.Configuration["BootstrapAdmin:Email"];
    var password = app.Configuration["BootstrapAdmin:Password"];

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        throw new InvalidOperationException("Falta configurar el correo o la contrasena en BootstrapAdmin en user secrets");
    }

    await using var scope = app.Services.CreateAsyncScope();

    var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();

    await seeder.SeedAdministratorAsync(email, password);

    app.Logger.LogInformation("Se ha inicializado la cuenta de administrador con el correo {Email}", email);

    return;
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Publica las rutas HTTP de los controladores
app.MapControllers();

app.MapPost("/Account/Logout", async Task<IResult> (
    HttpContext context,
    IAntiforgery antiforgery,
    SignInManager<ApplicationUser> signInManager) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(context);
    }
    catch (AntiforgeryValidationException)
    {
        return Results.BadRequest(
            "No se pudo validar la solicitud. Recarga la página e inténtalo nuevamente.");
    }

    await signInManager.SignOutAsync();

    return Results.LocalRedirect("/Account/Login");
})
.RequireAuthorization();

app.Run();

// Permite probar el servidor real con WebApplicationFactory.
public partial class Program { }
