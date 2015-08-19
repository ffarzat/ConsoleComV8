using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Declarations
{

    /// <summary>
    /// A variable declarator
    /// </summary>
    public class VariableDeclarator:Node
    {
        public Pattern Id { get; set; }

        public Expression Init { get; set; }
    }
}
