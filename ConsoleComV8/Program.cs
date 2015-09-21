using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

            #region Moment

            var otimizadorMoment = new Otimizador(100, 100, 10, "Moment", "ResultadosMoment");
            otimizadorMoment.LimparResultadosAnteriores();

            var otimizouMoment = otimizadorMoment.Otimizar("global.js", "core-test.js");

            Console.WriteLine("{0} otimizou? {1}", "Moment", otimizouMoment);

            #endregion

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete(60000);
            GC.Collect();

            #region Lodash

            var otimizadorLodash = new Otimizador(100, 100, 15, "Lodash", "ResultadosLodash");
            otimizadorLodash.LimparResultadosAnteriores();
            otimizadorLodash.UsarSetTimeout();


            var otimizouLodash = otimizadorLodash.Otimizar("lodash.js", "lodashTest.js");

            Console.WriteLine("{0} otimizou? {1}", "lodash", otimizouLodash);
            #endregion

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete(60000);
            GC.Collect();

            #region Underscore

            var otimizador = new Otimizador(100, 100, 8, "underscore", "ResultadosUnderscore");
            otimizador.LimparResultadosAnteriores();
            otimizador.UsarSetTimeout();

            var otimizou = otimizador.Otimizar("underscore.js", "underscoreTests.js");

            Console.WriteLine("{0} otimizou? {1}", "Underscore", otimizou);

            #endregion


            

        }

    }





}
