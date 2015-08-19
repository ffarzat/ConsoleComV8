using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{
    /// <summary>
    /// 
    /// </summary>
    public class ThrowStatement: Statement
    {
        /// <summary>
        /// Argumento ou exceção
        /// </summary>
        public Expression Argument { get; set; }
    }
}
