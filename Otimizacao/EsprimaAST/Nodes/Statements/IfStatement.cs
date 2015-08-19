using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{

    /// <summary>
    /// Representa um IF
    /// </summary>
    public class IfStatement:Statement
    {
        /// <summary>
        /// Condição
        /// </summary>
        public Expression Test { get; set; }

        /// <summary>
        /// Statement executado caso a condição seja verdadeira
        /// </summary>
        public Statement Consequent { get; set; }

        /// <summary>
        /// Statement executado caso a condição seja falsa
        /// </summary>
        public Statement Alternate { get; set; }
    }
}
