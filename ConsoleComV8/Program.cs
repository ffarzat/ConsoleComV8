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


namespace ConsoleComV8
{
    class Program
    {
        static void Main(string[] args)
        {

            ManagerPool.InitializeCurrentPool(new ManualManagerSettings { RuntimeMaxCount = 2 });

            using (var scope = new ManagerScope())
            {
                var engine = scope.RuntimeManager.GetEngine();
                engine.Execute("var i = 200;");
                Console.WriteLine(engine.Script.i);
            }


        }

    }





}
