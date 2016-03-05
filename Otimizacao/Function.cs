using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao
{
    /// <summary>
    /// Representa uma função a ser otimizada
    /// </summary>
    public class Function
    {
        /// <summary>
        /// Nome da Função
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Árvore em Json
        /// </summary>
        public string Ast { get; set; }

        /// <summary>
        /// Total de vezes em que é utilizada no código
        /// </summary>
        public int Total { get; set; }
    }
}
