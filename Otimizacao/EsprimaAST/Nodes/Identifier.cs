using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes
{
    /// <summary>
    /// Representa um identificador
    /// </summary>
    public class Identifier: Expression, IPattern 
    {
        /// <summary>
        /// Nome para identificar
        /// </summary>
        public string Name { get; set; }
    }
}
