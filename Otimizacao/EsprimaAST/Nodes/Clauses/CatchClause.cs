using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Clauses
{
    /// <summary>
    /// Catch Clause of a Try
    /// </summary>
    public class CatchClause: Node
    {
        /// <summary>
        /// Exceção
        /// </summary>
        public Pattern Param { get; set; }

        /// <summary>
        /// Representa o Corpo do Catch
        /// </summary>
        public Statement Body { get; set; }
    }
}
