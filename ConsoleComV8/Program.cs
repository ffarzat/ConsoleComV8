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
            var helperMoment = new JavascriptHelper("Moment", false, false);
            helperMoment.ExecutarTestes("global.js", "core-test.js");

            var helperLodash = new JavascriptHelper("Lodash", true, false);
            helperLodash.ExecutarTestes("lodash.js", "lodashTest.js");

            var helperUnderscore = new JavascriptHelper("underscore", true, false);
            helperUnderscore.ExecutarTestes("underscore.js", "underscoreTests.js");

            Console.Read();
        }

    }





}
