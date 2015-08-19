using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{
    /// <summary>
    /// Representa um return
    /// </summary>
    public class ReturnStatement:Statement
    {
        /// <summary>
        /// valor ou expressão de retorno
        /// </summary>
        public Expression Argument { get; set; }
    }
}
