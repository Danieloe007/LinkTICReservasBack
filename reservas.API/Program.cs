using reservas.Infrastructure.Data;
using reservas.Infrastructure.Repositories;
using reservas.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using reservas.Business.Utils;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Configurar el logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configurar la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrar AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Registrar repositorios
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();

// Agregar servicios de Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Reservas API",
        Version = "v1",
        Description = "API para gestionar reservas",
        Contact = new OpenApiContact
        {
            Name = "Daniel Espinosa",
            Email = "daniel.espinosa.recaman@gmail.com"
        }
    });
});


// Configurar el serializador JSON para preservar ciclos
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});


var app = builder.Build();

// Configurar middleware de Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware global de manejo de excepciones
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (EntidadNoEncontradaException ex)
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Ocurrió un error en el servidor." });
        // Registrar el error
        var logger = context.RequestServices.GetService<ILogger<Program>>();
        logger.LogError(ex, "Error no controlado.");
    }
});

// Mapear endpoints después de configurar el middleware

// Obtener todas las reservas con filtros opcionales
app.MapGet("/reservas", async (IReservaRepository reservaRepository, DateTime? fecha, Guid? servicioId, Guid? clienteId) =>
{
    var reservas = await reservaRepository.GetReservasAsync(fecha, servicioId, clienteId);
    return Results.Ok(reservas);
})
.WithName("GetReservas")
.WithOpenApi();


// Obtener una reserva por ID
app.MapGet("/reservas/{id}", async (IReservaRepository reservaRepository, Guid id) =>
{
    var reserva = await reservaRepository.GetReservaByIdAsync(id);
    return Results.Ok(reserva);
})
.WithName("GetReservaById")
.WithOpenApi();

// Crear una nueva reserva
app.MapPost("/reservas", async (IReservaRepository reservaRepository, ReservaCreateDto reservaDto) =>
{
    var reserva = new Reserva
    {
        FechaHora = reservaDto.FechaHora,
        ClienteId = reservaDto.ClienteId,
        ServicioId = reservaDto.ServicioId,
        NotasAdicionales = reservaDto.NotasAdicionales
    };

    await reservaRepository.AddReservaAsync(reserva);

    return Results.Created($"/reservas/{reserva.Id}", new
    {
        reserva.Id,
        reserva.FechaHora,
        reserva.ClienteId,
        reserva.ServicioId,
        reserva.Estado,
        reserva.NotasAdicionales
    });
})
.WithName("CreateReserva")
.WithOpenApi();


// Modificar una reserva existente
app.MapPut("/reservas/{id}", async (IReservaRepository reservaRepository, Guid id, Reserva reservaActualizada) =>
{
    reservaActualizada.Id = id;
    await reservaRepository.UpdateReservaAsync(reservaActualizada);
    return Results.Ok(reservaActualizada);
})
.WithName("UpdateReserva")
.WithOpenApi();

// Cancelar una reserva
app.MapDelete("/reservas/{id}", async (IReservaRepository reservaRepository, Guid id) =>
{
    await reservaRepository.DeleteReservaAsync(id);
    return Results.Ok($"Reserva con ID {id} ha sido cancelada.");
})
.WithName("DeleteReserva")
.WithOpenApi();

app.Run();
