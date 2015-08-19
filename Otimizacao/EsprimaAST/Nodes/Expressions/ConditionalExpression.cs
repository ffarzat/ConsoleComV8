using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Expressions
{
    /// <summary>
    /// A conditional expression, i.e., a ternary ?/: expression.
    /// </summary>
    public class ConditionalExpression: Expression
    {
        public Expression Test { get; set; }

        public Expression Alternate { get; set; }

        public Expression Consequent { get; set; }

    }
}
