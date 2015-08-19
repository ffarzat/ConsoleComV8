using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{
    /// <summary>
    /// Representa um Bloco
    /// </summary>
    public class BlockStatement:Statement
    {
        /// <summary>
        /// Representa o Corpo do Bloco
        /// </summary>
        public Statement Body { get; set; }
    }
}
