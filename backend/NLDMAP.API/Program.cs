using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLDMAP.Infrastructure.Identity;
using NLDMAP.Infrastructure.Persistence;
using NLDMAP.API.Identity;

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

var app = builder.Build();

if (app.Environment.IsDevelopment() &&
    app.Configuration.GetValue<bool>("SeedIdentity"))
{
    await IdentitySeeder.SeedAsync(
        app.Services,
        app.Configuration);

    app.Logger.LogInformation(
        "Roles y administrador inicial preparados correctamente.");

    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("BlazorFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();