using reservas.Business.Models;

namespace reservas.Infrastructure.Repositories
{
    public interface IReservaRepository
    {
        Task<IEnumerable<ReservaDto>> GetReservasAsync(DateTime? fecha, Guid? servicioId, Guid? clienteId);
        Task<ReservaDto> GetReservaByIdAsync(Guid id);
        Task AddReservaAsync(Reserva reserva);
        Task UpdateReservaAsync(Reserva reserva);
        Task DeleteReservaAsync(Guid id);
    }
}
