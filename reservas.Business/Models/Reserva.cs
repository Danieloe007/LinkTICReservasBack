using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reservas.Business.Models
{
    public class Reserva
    {
        public Guid Id { get; set; }
        public DateTime FechaHora { get; set; }
        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public Guid ServicioId { get; set; }
        public Servicio Servicio { get; set; }
        public string Estado { get; set; } 
        public string NotasAdicionales { get; set; }
    }
}
