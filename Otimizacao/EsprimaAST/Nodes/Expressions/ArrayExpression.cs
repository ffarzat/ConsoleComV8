using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Expressions
{

    /// <summary>
    /// An array expression.
    /// </summary>
    public class ArrayExpression: Expression
    {
        public List<Expression> Elements { get; set; }
    }
}
