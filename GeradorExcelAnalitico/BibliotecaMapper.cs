using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeradorExcelAnalitico
{
    /// <summary>
    /// Representa um conjunto de Rodadas por Tipo de algoritmo agrupadas 
    /// </summary>
    public class BibliotecaMapper
    {
        /// <summary>
        /// Nome da biblioteca (id né... )
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Guarda as Rodadas por Algoritmo
        /// </summary>
        public Dictionary<string, List<RodadaMapper>> Rodadas { get; set; }

        /// <summary>
        /// Construtor
        /// </summary>
        public BibliotecaMapper()
        {
            Rodadas = new Dictionary<string, List<RodadaMapper>>();
        }

    }
}
