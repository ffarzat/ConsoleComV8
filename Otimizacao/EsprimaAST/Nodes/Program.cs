using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes
{
    /// <summary>
    /// Representa um Programa
    /// </summary>
   public class Program : Node
    {
        /// <summary>
        /// Representa o Corpo do Programa
        /// </summary>
       public List<Statement> Body { get; set; }
    }
}
