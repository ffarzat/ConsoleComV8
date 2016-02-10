using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao
{
    public class No
    {
        public int Indice { get; set; }

        public string Codigo { get; set; }

        public string Tipo { get; set; }

        public No(int indice, string codigo, string tipo)
        {
            Indice = indice;
            Codigo = codigo;
            Tipo = tipo;
        }
    }
}
