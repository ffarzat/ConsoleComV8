using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Otimizacao.Javascript;

namespace ConsoleComV8
{
    class Program
    {
        static void Main(string[] args)
        {
            var helper = new JavascriptHelper(Environment.CurrentDirectory);
            helper.ExecutarTestes("lodash.js", "lodashTest.js");
        }

    }





}
