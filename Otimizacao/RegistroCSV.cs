using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao
{
    /// <summary>
    /// Representa uma linha no log formatado
    /// </summary>
    public class RegistroCsv
    {
        public int Geracao { get; set; }

        public string Individuo { get; set; }

        public string Operacao { get; set; }

        public double Fitness { get; set; }

        public string Tempo { get; set; }

        public int Testes { get; set; }
    }
}
