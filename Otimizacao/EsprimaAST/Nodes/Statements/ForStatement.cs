using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otimizacao.EsprimaAST.Nodes.Declarations;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{

    /// <summary>
    /// For
    /// </summary>
    public class ForStatement:Statement
    {
        public VariableDeclaration Init { get; set; }

        public Expression Test { get; set; }

        public Expression Update { get; set; }

        public Statement Body { get; set; }
    }
}
