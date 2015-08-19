using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Expressions
{

    /// <summary>
    /// A unary operator expression.
    /// </summary>
    public class UnaryExpression: Expression
    {
        public UnaryOperator Operator { get; set; }

        public bool Prefix { get; set; }

        public Expression Argument { get; set; }
    }
}
