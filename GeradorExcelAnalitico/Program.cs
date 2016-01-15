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
                    ProcessarDiretorioGa(gaDir, subdir);

                
            }
        }

        /// <summary>
        /// Processa a roda do GA
        /// </summary>
        /// <param name="directoryGa"></param>
        /// <param name="biblioteca"></param>
        private static void ProcessarDiretorioGa(DirectoryInfo directoryGa, DirectoryInfo biblioteca)
        {
            //pego o csv ou xsls
            var rodadas = new List<RodadaMapper>();
            var instanceFile = directoryGa.GetFiles().FirstOrDefault();

            if (instanceFile == null)
            {
                Console.WriteLine("Deveria existir um arquivo com as rodadas | {0}", directoryGa.FullName);
                Environment.Exit(-1);
            }

            if (instanceFile.Extension == ".csv")
            {
                rodadas = RecuperarRodadasDoCsv(instanceFile, biblioteca, directoryGa.Name);
            }

            Console.WriteLine("Processando {0} rodadas do algoritmo {1} da biblioteca {1}",rodadas.Count, "GA", biblioteca);
            
            foreach (var instanciaGa in rodadas)
            {
                if (instanciaGa.Operacao == "Clonagem")
                {
                    //não encontrou. Continua o original
                    instanciaGa.TempoOriginalComUnload = instanciaGa.TempoFinalComUnload;
                    instanciaGa.CaracteresOriginal = instanciaGa.CaracteresFinal;
                    instanciaGa.LocOriginal = instanciaGa.LocOriginal;
                }
                else
                {
                    var diretorioInstancia = directoryGa.GetDirectories().FirstOrDefault(d => d.Name.Contains(instanciaGa.Rodada));

                    if (diretorioInstancia != null)
                    {

                        //Pegar o valor do original dentro da instancia

                        //Incluir os valores
                    }    
                }

                

                

            }


            
        }

        /// <summary>
        /// Lê o csv e retorna uma lista com as rodadas
        /// </summary>
        /// <param name="instanceFile"></param>
        /// <param name="biblioteca"></param>
        /// <param name="algoritmo"></param>
        private static List<RodadaMapper> RecuperarRodadasDoCsv(FileInfo instanceFile, DirectoryInfo biblioteca, string algoritmo)
        {
            //Ler as rodadas
            var rodadas = new List<RodadaMapper>();

            using (var csv = new TextFieldParser(instanceFile.FullName))
            {
                csv.ReadLine();
                csv.ReadLine();

                csv.TextFieldType = FieldType.Delimited;
                csv.SetDelimiters(",");
                csv.HasFieldsEnclosedInQuotes = true;

                var totalLinhas = ContarLinhas(biblioteca.GetFiles().First().FullName);

                //Incluir ler o LOC e o caracteres original

                while (!csv.EndOfData)
                {
                    string[] currentRow = csv.ReadFields();
                    rodadas.Add(new RodadaMapper()
                        {
                            Algoritmo = algoritmo,
                            Biblioteca = biblioteca.Name,
                            Rodada = currentRow[0],
                            Individuo = currentRow[1],
                            Operacao = currentRow[2],
                            FitnessFinal = currentRow[3],
                            TempoFinalComUnload = currentRow[4],
                            Testes = currentRow[5],
                            LocOriginal = totalLinhas
                        });
                }
            }

            return rodadas;
        }

        /// <summary>
        /// Conta o número de linhas de um txt
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static int ContarLinhas(string path)
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            return lines.Count();
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
