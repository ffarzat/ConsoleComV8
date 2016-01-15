using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeradorExcelAnalitico
{
    /// <summary>
    /// Inicial
    /// </summary>
    class Program
    {
        /// <summary>
        /// Principal
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            //Diretório do experimento (raiz)
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

            ValidarDiretorios(fromDirectoryPath);
            ProcessarDiretorios(fromDirectoryPath);
        }

        /// <summary>
        /// Processa os arquivos em excel, conta os arquivos em Js e gera o relatório final
        /// </summary>
        /// <param name="fromDirectoryPath"></param>
        private static void ProcessarDiretorios(string fromDirectoryPath)
        {
            var dir = new DirectoryInfo(fromDirectoryPath);
            foreach (var subdir in dir.GetDirectories())
            {
                Console.WriteLine("     Processando : {0}", subdir.Name);

                //GA

                //HC
                
            }
        }

        /// <summary>
        /// Varre a estrutura e descobre as bibliotecas
        /// </summary>
        /// <param name="fromDirectoryPath"></param>
        private static void ValidarDiretorios(string fromDirectoryPath)
        {
            var dir = new DirectoryInfo(fromDirectoryPath);
            int totalLibraries = dir.GetDirectories().Count();
            Console.WriteLine("{0} de bibliotecas para processar",totalLibraries);

            foreach (var subdir in dir.GetDirectories())
            {
                Console.WriteLine("     {0}", subdir.Name);

                if (subdir.GetDirectories().Count() !=2)
                {
                    Console.WriteLine("Diretório da biblioteca fora do formato esperado");
                    Environment.Exit(-1); 
                }
            }
        }
    }
}
