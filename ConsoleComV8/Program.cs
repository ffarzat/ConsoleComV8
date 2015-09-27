using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearScript.Manager;
using Microsoft.ClearScript.V8;
using NLog;
using Otimizacao;
using Otimizacao.Javascript;


namespace ConsoleComV8
{

    /// <summary>
    /// Console de Execução
    /// </summary>
    class Program
    {
        /// <summary>
        /// Principal
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 30; i++)
            {
                Console.WriteLine("======================================   Rodada {0}", i);

                #region Moment

                //var otimizadorMoment = new Otimizador(100, 100, 10, "Moment", "ResultadosMoment");
                //otimizadorMoment.ConfigurarRodada(i);
                //otimizadorMoment.LimparResultadosAnteriores();

                //var otimizouMoment = otimizadorMoment.Otimizar("global.js", "core-test.js");

                //Console.WriteLine("{0} otimizou? {1}", "Moment", otimizouMoment);

                #endregion

                #region Lodash

                var otimizadorLodash = new Otimizador(100, 100, 15, "Lodash", "ResultadosLodash");
                otimizadorLodash.ConfigurarRodada(i);
                otimizadorLodash.LimparResultadosAnteriores();
                otimizadorLodash.UsarSetTimeout();

                var otimizouLodash = otimizadorLodash.Otimizar("lodash.js", "lodashTest.js");
                otimizadorLodash.Dispose();

                Console.WriteLine("{0} otimizou? {1}", "lodash", otimizouLodash);

                #endregion

                #region Underscore

                var otimizador = new Otimizador(100, 100, 8, "underscore", "ResultadosUnderscore");
                otimizador.ConfigurarRodada(i);
                otimizador.LimparResultadosAnteriores();
                otimizador.UsarSetTimeout();

                var otimizou = otimizador.Otimizar("underscore.js", "underscoreTests.js");
                otimizador.Dispose();

                Console.WriteLine("{0} otimizou? {1}", "Underscore", otimizou);

                Console.WriteLine("===================================//>   Rodada {0}", i);
                #endregion


                #region Força limpeza do GC
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.WaitForFullGCComplete(60000);
                GC.Collect();
                
                //Tire uma soneca
                Thread.Sleep(60000);

                #endregion

            }

            sw.Stop();

            Console.WriteLine("Rodadas executados com sucesso. Tempo total {0}", sw.Elapsed.ToString(@"hh\:mm\:ss\,ffff"));
        }
    }
}
