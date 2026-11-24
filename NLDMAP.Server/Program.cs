using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using Microsoft.EntityFrameworkCore;
using NLDMAP.Infrastructure;
using NLDMAP.Infrastructure.Identity;
using NLDMAP.Infrastructure.Catalogs;
using NLDMAP.Infrastructure.Persistence;
using NLDMAP.Server.Components;
using Microsoft.AspNetCore.Components.Authorization;
using NLDMAP.Server.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using MudBlazor.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    var proxy = builder.Configuration["ReverseProxy:KnownProxy"];
    if (!string.IsNullOrWhiteSpace(proxy)) options.KnownProxies.Add(IPAddress.Parse(proxy));
});
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
    builder.Services.AddDataProtection().SetApplicationName("NLDMAP")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Se registran los servicios para trabajar con los controladores
builder.Services.AddControllers();

// Se registran los servicios de identity y persistencia de datos
builder.Services.AddInfrastructure(builder.Configuration);

// El servicio de autenticación se configura para usar cookies y el esquema de identidad predeterminado
var authentication = builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme);
authentication.AddIdentityCookies();
authentication.AddBearerToken(MobileAuthController.Scheme, options =>
{
    options.BearerTokenExpiration = TimeSpan.FromMinutes(15);
    options.RefreshTokenExpiration = TimeSpan.FromDays(7);
});

// Se registran los servicios de para la comprobación de autorización y se habilita la política de autorización predeterminada
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(ReportCreationRequirement.Policy, policy =>
        policy.RequireAuthenticatedUser().AddRequirements(new ReportCreationRequirement()));
    foreach (var permission in PermissionNames.Catalog.Keys.Append(PermissionNames.Access))
        options.AddPolicy(permission, policy => policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(permission)));
});
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ReportCreationAuthorizationHandler>();
builder.Services.AddScoped<ICurrentActor, CurrentActor>();
builder.Services.AddScoped<AdministrationService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        HttpMethods.IsPost(context.Request.Method) &&
        (context.Request.Path.StartsWithSegments("/Account") ||
         context.Request.Path.StartsWithSegments("/cuenta") ||
         context.Request.Path.StartsWithSegments("/api/mobile/auth"))
            ? RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "local", _ =>
                new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })
            : RateLimitPartition.GetNoLimiter("other"));
});

// Se registran los servicios de autenticación y autorización para Blazor Server
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(30);
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Events.OnRedirectToLogin = context =>
    {
        var path = context.Request.Path;
        var citizenPage = path.StartsWithSegments("/cuenta") || path.StartsWithSegments("/reportes");
        context.Response.Redirect(citizenPage ? "/cuenta/entrar" : context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = async context =>
    {
        if (context.Request.Path.StartsWithSegments("/reportes"))
        {
            var authorization = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            var staff = await authorization.AuthorizeAsync(context.HttpContext.User, PermissionNames.Access);
            context.Response.Redirect(staff.Succeeded ? context.RedirectUri : "/cuenta/perfil");
            return;
        }
        context.Response.Redirect(context.RedirectUri);
    };
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

builder.Services.AddMemoryCache();

builder.Services.AddScoped<AdministrationAccessCache>();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("SeedCatalogs"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var inserted = await scope.ServiceProvider.GetRequiredService<MongoCatalogSeeder>().SeedAsync();
    app.Logger.LogInformation("Catálogos inicializados. Opciones nuevas: {Inserted}", inserted);
    return;
}

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/mobile"), web =>
    web.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));
app.UseHttpsRedirection();


app.MapStaticAssets()
    .ShortCircuit();

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
    if (!app.Environment.IsDevelopment() && !app.Configuration.GetValue<bool>("BootstrapAdmin:Enabled"))
    {
        throw new InvalidOperationException("Para inicializar Identity en producción habilita BootstrapAdmin:Enabled únicamente en el comando de inicialización.");
    }
    var email = app.Configuration["BootstrapAdmin:Email"];
    var password = app.Configuration["BootstrapAdmin:Password"];

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        throw new InvalidOperationException("Falta configurar BootstrapAdmin:Email y BootstrapAdmin:Password.");
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
app.MapGet("/health/live", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapPost("/Account/Logout", SignOutAsync).RequireAuthorization();
app.MapPost("/cuenta/salir", SignOutAsync).RequireAuthorization();

app.Run();

static async Task<IResult> SignOutAsync(
    HttpContext context,
    IAntiforgery antiforgery,
    SignInManager<ApplicationUser> signInManager)
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

    return Results.LocalRedirect(context.Request.Path.StartsWithSegments("/cuenta")
        ? "/cuenta/entrar" : "/Account/Login");
}
