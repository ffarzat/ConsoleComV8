using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{
    /// <summary>
    /// Representa um COntinue;
    /// </summary>
    public class ContinueStatement: Statement
    {
        /// <summary>
        /// Identificador [label, texto]
        /// </summary>
        public Identifier Label { get; set; }
    }
}
