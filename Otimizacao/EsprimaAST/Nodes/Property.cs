using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes
{
    /// <summary>
    /// A literal property in an object expression can have either a string or number as its value. 
    /// Ordinary property initializers have a kind value "init"; getters and setters have the kind values "get" and "set", respectively.
    /// </summary>
    public class Property:Node
    {
        /// <summary>
        /// Literal | Identifier
        /// </summary>
        public Identifier Key { get; set; }

        public Expression Value { get; set; }
        /// <summary>
        /// "init" | "get" | "set"
        /// </summary>
        public String Kind { get; set; }
    }
}
