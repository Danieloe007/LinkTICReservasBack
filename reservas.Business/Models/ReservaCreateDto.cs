using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reservas.Business.Models
{
    public class ReservaCreateDto
    {
        public DateTime FechaHora { get; set; }
        public Guid ClienteId { get; set; }
        public Guid ServicioId { get; set; }
        public string NotasAdicionales { get; set; }
    }
}
