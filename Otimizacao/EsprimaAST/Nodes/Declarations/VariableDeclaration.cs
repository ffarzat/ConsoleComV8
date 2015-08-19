using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Declarations
{

    /// <summary>
    /// A variable declaration.
    /// </summary>
    public class VariableDeclaration : Node, IDeclaration
    {
        public List<VariableDeclarator> Declarations { get; set; }

        /// <summary>
        /// Tipo de declaração
        /// </summary>
        public string Kind { get; internal set; }


        /// <summary>
        /// Construtor. Força o Kind para var
        /// </summary>
        public VariableDeclaration()
        {
            this.Kind = "var";
        }
    }
}
