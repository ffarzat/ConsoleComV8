using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Expressions
{

    /// <summary>
    /// A function or method call expression.
    /// </summary>
    public class CallExpression: Expression
    {
        public Expression Callee { get; set; }

        public List<Expression> Arguments { get; set; }
    }
}
