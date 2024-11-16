using reservas.Business.Models;
using reservas.Business.Utils;
using reservas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace reservas.Infrastructure.Repositories
{
    public class ReservaRepository : IReservaRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReservaRepository> _logger;

        public ReservaRepository(AppDbContext context, ILogger<ReservaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddReservaAsync(Reserva reserva)
        {
            try
            {
                // Validar existencia del Cliente
                var clienteExistente = await _context.Clientes.FindAsync(reserva.ClienteId);
                if (clienteExistente == null)
                {
                    throw new EntidadNoEncontradaException($"Cliente con ID {reserva.ClienteId} no encontrado.");
                }

                // Validar existencia del Servicio
                var servicioExistente = await _context.Servicios.FindAsync(reserva.ServicioId);
                if (servicioExistente == null)
                {
                    throw new EntidadNoEncontradaException($"Servicio con ID {reserva.ServicioId} no encontrado.");
                }

                // Generar nuevo ID y establecer estado
                reserva.Id = Guid.NewGuid();
                reserva.Estado = "Activa";

                // Agregar reserva
                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar una nueva reserva.");
                throw;
            }
        }
        public async Task UpdateReservaAsync(Reserva reserva)
        {
            try
            {
                var reservaExistente = await _context.Reservas.FindAsync(reserva.Id);
                if (reservaExistente == null)
                {
                    throw new EntidadNoEncontradaException($"Reserva con ID {reserva.Id} no encontrada.");
                }

                // Validar existencia del Cliente
                var clienteExistente = await _context.Clientes.FindAsync(reserva.ClienteId);
                if (clienteExistente == null)
                {
                    throw new EntidadNoEncontradaException($"Cliente con ID {reserva.ClienteId} no encontrado.");
                }

                // Validar existencia del Servicio
                var servicioExistente = await _context.Servicios.FindAsync(reserva.ServicioId);
                if (servicioExistente == null)
                {
                    throw new EntidadNoEncontradaException($"Servicio con ID {reserva.ServicioId} no encontrado.");
                }

                // Actualizar campos
                reservaExistente.FechaHora = reserva.FechaHora;
                reservaExistente.ClienteId = reserva.ClienteId;
                reservaExistente.ServicioId = reserva.ServicioId;
                reservaExistente.NotasAdicionales = reserva.NotasAdicionales;
                reservaExistente.Estado = "Modificada";

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar la reserva con ID {reserva.Id}.");
                throw;
            }
        }
        public async Task DeleteReservaAsync(Guid id)
        {
            try
            {
                var reserva = await _context.Reservas.FindAsync(id);
                if (reserva == null)
                {
                    throw new EntidadNoEncontradaException($"Reserva con ID {id} no encontrada.");
                }

                // Marcar la reserva como cancelada
                reserva.Estado = "Cancelada";

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cancelar la reserva con ID {id}.");
                throw;
            }
        }
        public async Task<ReservaDto> GetReservaByIdAsync(Guid id)
        {
            try
            {
                var reserva = await _context.Reservas
                    .AsNoTracking()
                    .Include(r => r.Cliente)
                    .Include(r => r.Servicio)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reserva == null)
                {
                    throw new EntidadNoEncontradaException($"Reserva con ID {id} no encontrada.");
                }

                // Mapear la entidad Reserva a ReservaDto
                var reservaDto = new ReservaDto
                {
                    Id = reserva.Id,
                    FechaHora = reserva.FechaHora,
                    Estado = reserva.Estado,
                    NotasAdicionales = reserva.NotasAdicionales,
                    Cliente = new ClienteDto
                    {
                        Id = reserva.Cliente.Id,
                        Nombre = reserva.Cliente.Nombre,
                        Apellido = reserva.Cliente.Apellido
                        // Puedes agregar más propiedades si lo deseas
                    },
                    Servicio = new ServicioDto
                    {
                        Id = reserva.Servicio.Id,
                        Nombre = reserva.Servicio.Nombre
                        // Puedes agregar más propiedades si lo deseas
                    }
                };

                return reservaDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener la reserva con ID {id}.");
                throw;
            }
        }
        public async Task<IEnumerable<ReservaDto>> GetReservasAsync(DateTime? fecha, Guid? servicioId, Guid? clienteId)
        {
            try
            {
                var query = _context.Reservas
                    .AsNoTracking()
                    .Include(r => r.Cliente)
                    .Include(r => r.Servicio)
                    .AsQueryable();

                if (fecha.HasValue)
                {
                    query = query.Where(r => r.FechaHora.Date == fecha.Value.Date);
                }

                if (servicioId.HasValue)
                {
                    query = query.Where(r => r.ServicioId == servicioId.Value);
                }

                if (clienteId.HasValue)
                {
                    query = query.Where(r => r.ClienteId == clienteId.Value);
                }

                var reservas = await query
                    .Select(r => new ReservaDto
                    {
                        Id = r.Id,
                        FechaHora = r.FechaHora,
                        Estado = r.Estado,
                        NotasAdicionales = r.NotasAdicionales,
                        Cliente = new ClienteDto
                        {
                            Id = r.Cliente.Id,
                            Nombre = r.Cliente.Nombre,
                            Apellido = r.Cliente.Apellido
                        },
                        Servicio = new ServicioDto
                        {
                            Id = r.Servicio.Id,
                            Nombre = r.Servicio.Nombre
                        }
                    })
                    .ToListAsync();

                return reservas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de reservas.");
                throw;
            }
        }

    }

}
