using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes
{
    /// <summary>
    /// Representa uma função
    /// </summary>
    public class Function: Node
    {
        /// <summary>
        /// Id da Função
        /// </summary>
        public Identifier Id { get; set; }

        /// <summary>
        /// Parametros da Função
        /// </summary>
        public Pattern Params { get; set; }

        /// <summary>
        /// Representa o Corpo da Funçao
        /// </summary>
        public Statement Body { get; set; }

    }
}
