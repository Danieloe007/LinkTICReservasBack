using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reservas.Business.Models
{
    public class ReservaDto
    {
        public Guid Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; }
        public string NotasAdicionales { get; set; }
        public ClienteDto Cliente { get; set; }
        public ServicioDto Servicio { get; set; }
    }

}
