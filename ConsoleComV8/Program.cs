using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Otimizacao.EsprimaAST.Json;
using Otimizacao.Javascript;

namespace ConsoleComV8
{
    class Program
    {
        static void Main(string[] args)
        {


            var sw = new Stopwatch();
            sw.Start();

            var helper = new JavascriptHelper(Environment.CurrentDirectory);

            helper.ExecutarTestes();

            sw.Stop();
            
            Console.WriteLine("{0} segundos totais", sw.Elapsed.Seconds);
            Console.Read();

        }

    }





}
