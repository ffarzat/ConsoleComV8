using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeradorExcelAnalitico
{
    /// <summary>
    /// Representa uma linha de código Alterada ou excluída
    /// </summary>
    public class LinhaAlterada
    {
        /// <summary>
        /// Codigo
        /// </summary>
        public string Linha { get; set; }
        
        /// <summary>
        /// Repeticao
        /// </summary>
        public int Frequencia { get; set; }
        
        /// <summary>
        /// Deleted or Modified
        /// </summary>
        public string Alteracao { get; set; }

    }
}
