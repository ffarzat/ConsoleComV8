using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Expressions
{

    /// <summary>
    /// A member expression. 
    /// If computed is true, the node corresponds to a computed (a[b]) member expression and property is an Expression. 
    /// If computed is false, the node corresponds to a static (a.b) member expression and property is an Identifier.
    /// </summary>
    public class MemberExpression: Expression, IPattern
    {
        public Expression Object { get; set; }

        public Expression Property { get; set; }

        public bool Computed { get; set; }
    }
}
