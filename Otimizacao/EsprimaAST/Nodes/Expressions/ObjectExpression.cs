using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Expressions
{

    /// <summary>
    /// An object expression.
    /// </summary>
    public class ObjectExpression:Expression
    {
        /// <summary>
        /// Lista das propriedades
        /// </summary>
        public List<Property> Properties { get; set; }
    }
}
