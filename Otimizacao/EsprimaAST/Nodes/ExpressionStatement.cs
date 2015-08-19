using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes
{
    /// <summary>
    /// Representa um Statement do tipo Expressao
    /// </summary>
    public class ExpressionStatement: Statement
    {
        /// <summary>
        /// Garda a expressão desse Statement
        /// </summary>
        public Expression Expression { get; set; }

        /// <summary>
        /// Construtor
        /// </summary>
        public ExpressionStatement()
        {
            this.Type = "ExpressionStatement";
        }
    }
}
