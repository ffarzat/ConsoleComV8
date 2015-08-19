using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{
    /// <summary>
    /// Representa uma expressão
    /// </summary>
    public class ExpressionStatement:Statement
    {
        /// <summary>
        /// Guarda a expressão
        /// </summary>
        public Expression Expression { get; set; }
    }
}
