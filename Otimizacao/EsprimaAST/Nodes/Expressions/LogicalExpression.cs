using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Expressions
{
    /// <summary>
    /// A logical operator expression.
    /// </summary>
    public class LogicalExpression: Expression
    {
        public LogicalOperator Operator { get; set; }

        public Expression Left { get; set; }

        public Expression Rigth { get; set; }
    }
}
