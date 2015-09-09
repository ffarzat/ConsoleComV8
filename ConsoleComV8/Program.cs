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
            var otimizador = new Otimizador(10, 5, 20, "underscore", "ResultadosUnderscore");
            otimizador.UsarSetTimeout();
            
            var otimizou = otimizador.Otimizar("underscore.js", "underscoreTests.js");
            

        }

    }





}
