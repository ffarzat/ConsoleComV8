using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes
{
    /// <summary>
    /// A literal token. Note that a literal can be an expression.
    /// </summary>
    public class Literal: Expression
    {
        /// <summary>
        /// string | boolean | null | number | RegExp;
        /// </summary>
        public object Value { get; set; }

    }
}
