using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{

    /// <summary>
    /// Representa um with
    /// </summary>
    public class WithStatement: Statement
    {
        /// <summary>
        /// Objeto alvo do with
        /// </summary>
        public Expression Object { get; set; }

        /// <summary>
        /// Representa o Corpo do with
        /// </summary>
        public Statement Body { get; set; }
    }
}
