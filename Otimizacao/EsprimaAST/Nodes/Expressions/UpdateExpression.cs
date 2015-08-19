using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Expressions
{
    /// <summary>
    /// An update (increment or decrement) operator expression.
    /// </summary>
    public class UpdateExpression: Expression
    {
        public UpdateOperator Operator { get; set; }

        public Expression Argument { get; set; }

        public bool Prefix { get; set; }
    }
}
