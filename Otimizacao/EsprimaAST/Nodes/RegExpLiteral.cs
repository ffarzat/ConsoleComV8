using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes
{
    /// <summary>
    /// The regex property allows regexes to be represented in environments that don’t support certain flags such as y or u. 
    /// In environments that don't support these flags value will be null as the regex can't be represented natively.
    /// </summary>
    public class RegExpLiteral:Literal
    {
        /// <summary>
        /// Representa um expressão regular
        /// </summary>
        public struct Regex
        {
            public string Pattern { get; set; }
            public string Flags { get; set; }
        }
    }
}
