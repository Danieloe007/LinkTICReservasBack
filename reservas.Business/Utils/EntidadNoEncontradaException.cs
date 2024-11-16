using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reservas.Business.Utils
{
    public class EntidadNoEncontradaException : Exception
    {
        public EntidadNoEncontradaException()
        {
        }

        public EntidadNoEncontradaException(string message)
            : base(message)
        {
        }

        public EntidadNoEncontradaException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
