using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeradorExcelAnalitico
{
    class Program
    {
        static void Main(string[] args)
        {

            var fromDirectoryPath = args[0];

            if(string.IsNullOrEmpty(fromDirectoryPath))
            {    
                Console.WriteLine("Diretório do experimento não informado");
                Environment.Exit(-1);
            }

            if (!Directory.Exists(fromDirectoryPath))
            {
                Console.WriteLine("Diretório do experimento não existe | {0}", fromDirectoryPath);
                Environment.Exit(-1);
            }

            


        }
    }
}
