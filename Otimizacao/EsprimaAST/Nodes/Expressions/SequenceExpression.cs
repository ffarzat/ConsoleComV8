using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Expressions
{
    /// <summary>
    /// A sequence expression, i.e., a comma-separated sequence of expressions.
    /// </summary>
    public class SequenceExpression: Expression
    {

        /// <summary>
        /// 
        /// </summary>
        public List<Expression> Expressions { get; set; }
    }
}
