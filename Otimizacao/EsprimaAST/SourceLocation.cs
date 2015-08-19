using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST
{
    /// <summary>
    /// Reprenta uma localização de código na árvore
    /// </summary>
    public class SourceLocation
    {
        /// <summary>
        /// Código fonte
        /// </summary>
        public string Source { get; set; }
        
        /// <summary>
        /// Posição Inicial
        /// </summary>
        public Position Start { get; set; }
        
        /// <summary>
        /// Posição Final
        /// </summary>
        public Position End{ get; set; }
    }
}
