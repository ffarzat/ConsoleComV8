using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace GeradorExcelAnalitico
{
    /// <summary>
    /// Inicial
    /// </summary>
    class Program
    {
        /// <summary>
        /// Guarda o diretório raiz. Os exceis resultado ficarão gravados aqui
        /// </summary>
        private static string _baseDirectory;

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

            _baseDirectory = fromDirectoryPath;
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

                var gaDir = subdir.GetDirectories().FirstOrDefault(n => n.Name == "GA");
                var hcDir = subdir.GetDirectories().FirstOrDefault(n => n.Name == "HC");

                if (gaDir != null)
                    ProcessarDiretorioGa(gaDir);

                
            }
        }

        /// <summary>
        /// Processa a roda do GA
        /// </summary>
        /// <param name="directoryGa"></param>
        private static void ProcessarDiretorioGa(DirectoryInfo directoryGa)
        {
            //pego o csv ou xsls

            var instanceFile = directoryGa.GetFiles().FirstOrDefault();

            if (instanceFile == null)
            {
                Console.WriteLine("Deveria existir um arquivo com as rodadas | {0}", directoryGa.FullName);
                Environment.Exit(-1);
            }

            if (instanceFile.Extension == ".csv")
            {
                //Ler as rodadas

                using (var csv = new TextFieldParser(instanceFile.FullName))
                {
                    csv.ReadLine();
                    csv.ReadLine();

                    csv.TextFieldType = FieldType.Delimited;
                    csv.SetDelimiters(",");
                    csv.HasFieldsEnclosedInQuotes = true;

                    while (!csv.EndOfData)
                    {
                        string[] currentRow = csv.ReadFields();
                        Console.WriteLine("{0} - {1}", currentRow[0], currentRow[3]);
                    }
                }
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
