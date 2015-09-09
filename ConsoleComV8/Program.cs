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
            for (int i = 0; i < 60; i++)
            {
                var helperMoment = new JavascriptHelper("Moment", false, false);

                //criar os individuos

                //Fazer o setup da FitNess

                //Configurar as rodadas, log, relatório de saída

                //Executar

                
            }



            Console.Read();
        }

    }





}
