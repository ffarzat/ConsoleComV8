using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;

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
                rodadas = RecuperarRodadasDoCsv(instanceFile, biblioteca, directoryGa);
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
        /// <param name="dirGa"></param>
        private static List<RodadaMapper> RecuperarRodadasDoCsv(FileInfo instanceFile, DirectoryInfo biblioteca, DirectoryInfo dirGa)
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

                //Conta das linhas do original
                var totalLinhas = ContarLinhas(biblioteca.GetFiles().First().FullName);
                //Conta Tokens do Original
                var totalchars = GetNumOfCharsInFile(biblioteca.GetFiles().First().FullName);

                while (!csv.EndOfData)
                {
                    string[] currentRow = csv.ReadFields();

                    string bestFile = Path.GetFileName(currentRow[1]);
                    int totalLinhasBest = 0;
                    int totalcharsBest = 0;
                    var fileList = dirGa.GetFiles(bestFile, SearchOption.AllDirectories);

                    if (fileList.Any())
                    {
                        totalLinhasBest = ContarLinhas(fileList.First().FullName);
                        totalcharsBest = GetNumOfCharsInFile(fileList.First().FullName);
                    }


                    rodadas.Add(new RodadaMapper()
                        {
                            Algoritmo = dirGa.Name,
                            Biblioteca = biblioteca.Name,
                            Rodada = currentRow[0],
                            Individuo = currentRow[1],
                            Operacao = currentRow[2],
                            FitnessFinal = currentRow[3],
                            TempoFinalComUnload = currentRow[4],
                            Testes = currentRow[5],
                            LocOriginal = totalLinhas,
                            LocFinal = totalLinhasBest,
                            CaracteresOriginal = totalchars,
                            CaracteresFinal = totalcharsBest
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
        /// Conta o número de caracteres de um arquivo texto
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int GetNumOfCharsInFile(string filePath)
        {
            int count = 0;
            using (var sr = new StreamReader(filePath))
            {
                while (sr.Read() != -1)
                    count++;
            }
            return count;
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
