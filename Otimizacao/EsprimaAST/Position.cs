using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST
{

    /// <summary>
    /// Representa a localização de um pedaço do código fonte
    /// </summary>
    public abstract class Position
    {
        /// <summary>
        /// Linha
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Coluna
        /// </summary>
        public int Column { get; set; }

    }
}
