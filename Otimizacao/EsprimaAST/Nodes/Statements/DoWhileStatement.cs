using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{

    /// <summary>
    /// A do/while statement
    /// </summary>
    public class DoWhileStatement:Statement
    {
        /// <summary>
        /// Condição
        /// </summary>
        public Expression Test { get; set; }

        /// <summary>
        /// Corpo
        /// </summary>
        public Statement Body { get; set; }
    }
}
