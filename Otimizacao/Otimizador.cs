using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Otimizacao.Javascript;

namespace Otimizacao
{
    /// <summary>
    /// Representa um otimizador de javascript
    /// </summary>
    public class Otimizador
    {
        /// <summary>
        /// NLog Logger
        /// </summary>
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Timeout em segundos para avaliação de um individuo
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// Lista dos individuos atuais
        /// </summary>
        private List<Individuo> _population = new List<Individuo>();

        /// <summary>
        /// Construtor Default
        /// </summary>
        public Otimizador()
        {
            TimeOut = 20;
        }


        /// <summary>
        /// Executa uma mutação no individuo
        /// </summary>
        /// <param name="sujeito"> </param>
        private Individuo ExecutarMutacao(Individuo sujeito)
        {
            return sujeito;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pai"></param>
        /// <param name="mae"></param>
        /// <param name="filhoPai"></param>
        /// <param name="filhoMae"></param>
        private void ExecutarCruzamento(Individuo pai, Individuo mae, out Individuo filhoPai, out Individuo filhoMae)
        {
            filhoPai = null;
            filhoMae = null;
        }

        /// <summary>
        /// Executa os testes com o sujeito e preenche sua propriedade Fitness Retorna o valor dessa propriedade
        /// </summary>
        /// <param name="sujeito"></param>
        private Int64 AvaliarIndividuo(Individuo sujeito)
        {
            return sujeito.Fitness;
        }
    }
}
