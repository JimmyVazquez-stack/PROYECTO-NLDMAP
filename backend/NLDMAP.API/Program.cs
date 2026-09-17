using NLDMAP.Application;
using NLDMAP.Infrastructure;
using NLDMAP.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

//registro de servicios
//habilitar controladores
builder.Services.AddControllers();

//habilitar swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Inyección de la infraestructura (Base de datos)
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
    {
        options.AddPolicy("PoliticaFrontend", policy =>
        {
            policy.AllowAnyOrigin() //url exacta de blazor
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });


var app = builder.Build();

//Middlewares
//mostrar swagger solo en entorno desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("PoliticaFrontend");

app.UseAuthorization();

//mapear rutas de controladores
app.MapControllers();

// 2. Endpoint de prueba rápida
app.MapGet("/", () => "¡La API está corriendo y la Infraestructura se inyectó correctamente!");

app.Run();