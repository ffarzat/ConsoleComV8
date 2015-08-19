using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otimizacao.EsprimaAST.Nodes.Clauses;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{
    /// <summary>
    /// Representa um Switch
    /// </summary>
    public class SwitchStatement: Statement
    {
        /// <summary>
        /// Variável ou valor de controle
        /// </summary>
        public Expression Discriminant { get; set; }
        
        /// <summary>
        /// Lista dos cases do Switch
        /// </summary>
        public SwitchCase Cases { get; set; }
    }
}
