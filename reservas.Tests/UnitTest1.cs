using reservas.Infrastructure.Data;
using reservas.Infrastructure.Repositories;
using reservas.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Threading.Tasks;
using Xunit;
using reservas.Business.Utils;

namespace reservas.Tests
{
    public class ReservaRepositoryTests
    {
        private readonly ILogger<ReservaRepository> _logger;

        public ReservaRepositoryTests()
        {
            // Crear un logger de prueba
            _logger = new LoggerFactory().CreateLogger<ReservaRepository>();
        }

        [Fact]
        public async Task AddReservaAsync_ShouldAddReserva_WhenClienteAndServicioExist()
        {
            // Configurar el DbContextOptions para ignorar las advertencias de transacción
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .EnableSensitiveDataLogging()
                .Options;

            // Arrange
            using var context = new AppDbContext(options);
            var repository = new ReservaRepository(context, _logger);

            // Crear entidades relacionadas con todas las propiedades requeridas
            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nombre = "Juan",
                Apellido = "Pérez",
                Email = "juan.perez@example.com",
                Telefono = "123456789"
            };

            var servicio = new Servicio
            {
                Id = Guid.NewGuid(),
                Nombre = "Servicio de prueba",
                Descripcion = "Descripción del servicio",
                Precio = 100.0M
            };

            context.Clientes.Add(cliente);
            context.Servicios.Add(servicio);
            await context.SaveChangesAsync();

            var reserva = new Reserva
            {
                FechaHora = DateTime.Now,
                ClienteId = cliente.Id,
                ServicioId = servicio.Id,
                NotasAdicionales = "Sin notas"
            };

            // Act
            await repository.AddReservaAsync(reserva);

            // Assert
            var reservaEnDb = await context.Reservas.FindAsync(reserva.Id);
            Assert.NotNull(reservaEnDb);
            Assert.Equal("Activa", reservaEnDb.Estado);
        }

        [Fact]
        public async Task AddReservaAsync_ShouldThrowException_WhenClienteDoesNotExist()
        {
            // Configurar el DbContextOptions para ignorar las advertencias de transacción
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .EnableSensitiveDataLogging()
                .Options;

            // Arrange
            using var context = new AppDbContext(options);
            var repository = new ReservaRepository(context, _logger);

            var servicio = new Servicio
            {
                Id = Guid.NewGuid(),
                Nombre = "Servicio de prueba",
                Descripcion = "Descripción del servicio",
                Precio = 100.0M
            };

            context.Servicios.Add(servicio);
            await context.SaveChangesAsync();

            var reserva = new Reserva
            {
                FechaHora = DateTime.Now,
                ClienteId = Guid.NewGuid(), // Cliente no existente
                ServicioId = servicio.Id,
                NotasAdicionales = "Sin notas"
            };

            // Act & Assert
            await Assert.ThrowsAsync<EntidadNoEncontradaException>(() => repository.AddReservaAsync(reserva));
        }

        [Fact]
        public async Task UpdateReservaAsync_ShouldUpdateReserva_WhenDataIsValid()
        {
            // Configurar el DbContextOptions para ignorar las advertencias de transacción
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .EnableSensitiveDataLogging()
                .Options;

            // Arrange
            using var context = new AppDbContext(options);
            var repository = new ReservaRepository(context, _logger);

            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nombre = "Juan",
                Apellido = "Pérez",
                Email = "juan.perez@example.com",
                Telefono = "123456789"
            };

            var servicio = new Servicio
            {
                Id = Guid.NewGuid(),
                Nombre = "Servicio de prueba",
                Descripcion = "Descripción del servicio",
                Precio = 100.0M
            };

            context.Clientes.Add(cliente);
            context.Servicios.Add(servicio);
            await context.SaveChangesAsync();

            var reserva = new Reserva
            {
                Id = Guid.NewGuid(),
                FechaHora = DateTime.Now,
                ClienteId = cliente.Id,
                ServicioId = servicio.Id,
                NotasAdicionales = "Sin notas",
                Estado = "Activa"
            };
            context.Reservas.Add(reserva);
            await context.SaveChangesAsync();

            // Modificar la reserva
            reserva.NotasAdicionales = "Notas actualizadas";

            // Act
            await repository.UpdateReservaAsync(reserva);

            // Assert
            var reservaEnDb = await context.Reservas.FindAsync(reserva.Id);
            Assert.Equal("Notas actualizadas", reservaEnDb.NotasAdicionales);
            Assert.Equal("Modificada", reservaEnDb.Estado);
        }
    }
}
