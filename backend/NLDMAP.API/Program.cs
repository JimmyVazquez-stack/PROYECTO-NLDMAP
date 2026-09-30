using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLDMAP.Infrastructure.Identity;
using NLDMAP.Infrastructure.Persistence;
using NLDMAP.API.Identity;

using System.Net;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Conexión para almacenar las cuentas del sistema.
var connectionString =
    builder.Configuration.GetConnectionString("IdentityConnection")
    ?? throw new InvalidOperationException(
        "Falta configurar ConnectionStrings:IdentityConnection.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Usuarios, roles y autenticación mediante cookies.
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 10;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7101")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Reconocer las cabeceras enviadas por nuestro proxy.
var proxyIp = builder.Configuration["ReverseProxy:KnownProxy"];

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.ForwardLimit = 1;

    if (!string.IsNullOrWhiteSpace(proxyIp))
    {
        options.KnownProxies.Add(IPAddress.Parse(proxyIp));
    }
});

// En producción, esta carpeta estará en un volumen persistente.
var keysPath = builder.Configuration["DataProtection:KeysPath"];

if (!string.IsNullOrWhiteSpace(keysPath))
{
    builder.Services
        .AddDataProtection()
        .SetApplicationName("NLDMAP")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

var app = builder.Build();

var migrateDatabase =
    app.Configuration.GetValue<bool>("MigrateDatabase");

var seedIdentity =
    app.Configuration.GetValue<bool>("SeedIdentity");

// Estos comandos realizan su tarea y terminan sin arrancar el servidor.
if (migrateDatabase || seedIdentity)
{
    if (migrateDatabase)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        await db.Database.MigrateAsync();

        app.Logger.LogInformation(
            "Migraciones aplicadas correctamente.");
    }

    if (seedIdentity)
    {
        await IdentitySeeder.SeedAsync(
            app.Services,
            app.Configuration);

        app.Logger.LogInformation(
            "Roles y administrador inicial preparados correctamente.");
    }

    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("BlazorFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/health/live", () =>
    Results.Ok(new { status = "ok" }))
    .AllowAnonymous();

app.Run();