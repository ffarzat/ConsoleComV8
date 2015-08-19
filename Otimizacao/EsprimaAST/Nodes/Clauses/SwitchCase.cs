using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Clauses
{
    /// <summary>
    /// Representa uma Case em um Switch
    /// </summary>
    public class SwitchCase:Node
    {
        /// <summary>
        /// Condição
        /// </summary>
        public Expression Test { get; set; }

        /// <summary>
        /// Statement executado caso o Test seja verdadeiro
        /// </summary>
        public Statement Consequent { get; set; }
    }
}
