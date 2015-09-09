using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.Javascript
{
    /// <summary>
    /// Representa um individuo na Iteração
    /// </summary>
    public class Individuo
    {
        /// <summary>
        /// Código final do individuo
        /// </summary>
        public string Codigo { get; set; }

        /// <summary>
        /// Arvore abstrata do individuo
        /// </summary>
        public string Ast { get; set; }

        /// <summary>
        /// Valor de Fitness calculado ao final
        /// </summary>
        public Int64 Fitness { get; set; }

        /// <summary>
        /// Construtor Default
        /// </summary>
        public Individuo()
        {
            Fitness = Int64.MaxValue;
        }
        /// <summary>
        /// Gera um clone do individuo atual
        /// </summary>
        /// <returns></returns>
        public Individuo Clone()
        {
            return new Individuo()
                {
                    Ast = this.Ast,
                    Codigo= this.Codigo,
                    Fitness = this.Fitness
                };
        }

    }
}
