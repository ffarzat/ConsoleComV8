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
            #region Underscore

            //var otimizador = new Otimizador(10, 5, 20, "underscore", "ResultadosUnderscore");
            //otimizador.UsarSetTimeout();
            
            //var otimizou = otimizador.Otimizar("underscore.js", "underscoreTests.js");
            #endregion

            #region Lodash

            //var otimizadorLodash = new Otimizador(10, 5, 20, "lodash", "ResultadosLodash");
            //otimizadorLodash.UsarSetTimeout();

            //var otimizouLodash = otimizadorLodash.Otimizar("lodash.js", "lodashTest.js");
            #endregion

            #region Moment
            var otimizadorMoment = new Otimizador(10, 5, 20, "Moment", "ResultadosMoment");
            //otimizadorMoment.UsarSetTimeout();

            var otimizouMoment = otimizadorMoment.Otimizar(Path.Combine(Environment.CurrentDirectory, "Moment", "global.js"), "core-test.js");

            #endregion

        }

    }





}
