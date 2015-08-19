using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Expressions
{

    /// <summary>
    /// A binary operator expression.
    /// </summary>
    public class BinaryExpression: Expression
    {
        public BinaryOperator Operator { get; set; }

        public Expression Left { get; set; }

        public Expression Rigth { get; set; }

    }
}
