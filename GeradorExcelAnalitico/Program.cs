﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Excel;
using Microsoft.VisualBasic.FileIO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SearchOption = System.IO.SearchOption;

namespace GeradorExcelAnalitico
{
    /// <summary>
    /// Inicial
    /// </summary>
    class Program
    {
        /// <summary>
        /// Armazena o estado das rodadas por biblioteca
        /// </summary>
        public static List<BibliotecaMapper> Bibliotecas { get; set; }

        /// <summary>
        /// Principal
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            //Diretório do experimento (raiz)
            var fromDirectoryPath = args[0];
            var resultsDirectory = args[1];


            if(string.IsNullOrEmpty(fromDirectoryPath))
            {    
                Console.WriteLine("Diretório do experimento não informado");
                Environment.Exit(-1);
            }

            if (string.IsNullOrEmpty(resultsDirectory))
            {
                Console.WriteLine("Diretório para slavar os resultados não informado");
                Environment.Exit(-1);
            }

            if (!Directory.Exists(fromDirectoryPath))
            {
                Console.WriteLine("Diretório do experimento não existe | {0}", fromDirectoryPath);
                Environment.Exit(-1);
            }

            if (!Directory.Exists(resultsDirectory))
            {
                Directory.CreateDirectory(resultsDirectory);
            }

            ValidarDiretorios(fromDirectoryPath);
            Bibliotecas = new List<BibliotecaMapper>();
            ProcessarDiretorios(fromDirectoryPath, resultsDirectory);
            ExportarPlanilhaEstatistica(resultsDirectory);
            
            //Console.Read();
        }

        /// <summary>
        /// Cria a planilha com os resultados calculados
        /// </summary>
        /// <param name="resultsDirectory"></param>
        private static void ExportarPlanilhaEstatistica(string resultsDirectory)
        {
            var excelAnalise = new FileInfo(resultsDirectory + @"\Analises.xlsx");

            if(excelAnalise.Exists)
                File.Delete(excelAnalise.FullName);

            var pck = new ExcelPackage(excelAnalise);
            Console.WriteLine("Montando planilha de Analises");

            //Guarda as linhas e 
            var mudancasComputadas = new List<LinhaAlterada>();

            foreach (var biblioteca in Bibliotecas)
            {
                Console.WriteLine(" worksheet {0}", biblioteca.Nome);

                mudancasComputadas.Clear();
                mudancasComputadas = new List<LinhaAlterada>();

                var ws = pck.Workbook.Worksheets.Add(biblioteca.Nome);
                
                #region Formatar Planilha

                ws.Cells["C1"].Value = "Tempo (%)";
                ws.Cells["C1:D1"].Merge = true;
                ws.Cells["C1:D1"].Style.Font.Bold = true; 
                ws.Cells["C1:D1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["F1"].Value = "LOC";
                ws.Cells["F1:G1"].Merge = true;
                ws.Cells["F1:G1"].Style.Font.Bold = true;
                ws.Cells["F1:G1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["I1"].Value = "Caracteres";
                ws.Cells["I1:J1"].Merge = true;
                ws.Cells["I1:J1"].Style.Font.Bold = true;
                ws.Cells["I1:J1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                ws.Cells["A2"].Value = "Rodadas";
                ws.Cells["A2"].Style.Font.Bold = true; 
                ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells["A34"].Value = "Média";
                ws.Cells["A34"].AddComment("Aqui quanto maior melhor. Trata-se do valor da diferença média (Original - Melhor", "ffarzat");
                ws.Cells["A34"].Style.Font.Bold = true;
                ws.Cells["A34"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells["A35"].Value = "Desvio Padrão";
                ws.Cells["A35"].Style.Font.Bold = true;
                ws.Cells["A35"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells["A39"].Value = "Linha de Código";
                ws.Cells["A39"].Style.Font.Bold = true;
                ws.Cells["A39"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["B39"].Value = "Tipo de Alteração";
                ws.Cells["B39"].Style.Font.Bold = true;
                ws.Cells["B39"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["C39"].Value = "Frequência";
                ws.Cells["C39"].AddComment("Quantas vezes apareceu no total dentro das 30 rodadas nos dois algortimos", "ffarzat");
                ws.Cells["C39"].Style.Font.Bold = true;
                ws.Cells["C39"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                

                ws.Cells["C2"].Value = "HC";
                ws.Cells["C2"].Style.Font.Bold = true;
                ws.Cells["C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["D2"].Value = "GA";
                ws.Cells["D2"].Style.Font.Bold = true;
                ws.Cells["D2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["F2"].Value = "HC";
                ws.Cells["F2"].Style.Font.Bold = true;
                ws.Cells["F2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["G2"].Value = "GA";
                ws.Cells["G2"].Style.Font.Bold = true;
                ws.Cells["G2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["I2"].Value = "HC";
                ws.Cells["I2"].Style.Font.Bold = true;
                ws.Cells["I2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["J2"].Value = "GA";
                ws.Cells["J2"].Style.Font.Bold = true;
                ws.Cells["J2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                #endregion

                for (int i = 1; i < 31; i++)
                {
                    string celulaRodada = "A" + (i + 2).ToString();
                    ws.Cells[celulaRodada].Value = i;

                    var rodadaHc = biblioteca.Rodadas["HC"].FirstOrDefault(r => r.Rodada == i.ToString());
                    var rodadaGa = biblioteca.Rodadas["GA"].FirstOrDefault(r => r.Rodada == i.ToString());

                    #region HC
                    if (rodadaHc !=null)
                    {
                        var diferencaTempoComUnload = (Decimal.Parse(rodadaHc.Fitness) / Decimal.Parse(rodadaHc.FitnessFinal) - 1) * 100;

                        string celulaTempoComUnload = "C" + (i + 2).ToString();
                        ws.Cells[celulaTempoComUnload].Style.Numberformat.Format = null;
                        ws.Cells[celulaTempoComUnload].Style.Numberformat.Format = "###,###,##0.00";
                        ws.Cells[celulaTempoComUnload].Value = diferencaTempoComUnload == 0 ? diferencaTempoComUnload : Decimal.Parse(diferencaTempoComUnload.ToString("#.##")); 

                        var diferencaLoc = rodadaHc.LocOriginal - rodadaHc.LocFinal;

                        string celulaLoc = "F" + (i + 2).ToString();
                        ws.Cells[celulaLoc].Style.Numberformat.Format = null;
                        ws.Cells[celulaLoc].Style.Numberformat.Format = "###,###,##0";
                        ws.Cells[celulaLoc].Value = diferencaLoc;

                        var diferencatokens = rodadaHc.CaracteresOriginal - rodadaHc.CaracteresFinal;

                        string celulaTokens = "I" + (i + 2).ToString();
                        ws.Cells[celulaTokens].Style.Numberformat.Format = null;
                        ws.Cells[celulaTokens].Style.Numberformat.Format = "###,###,##0";
                        ws.Cells[celulaTokens].Value = diferencatokens;

                        if (rodadaHc.Diferencas != null)
                        {
                            foreach (var linha in rodadaHc.Diferencas.Lines)
                            {
                                if (linha.Text.Trim().Equals(";"))
                                    continue;
                                
                                if (linha.Type == ChangeType.Deleted || linha.Type == ChangeType.Modified || linha.Type == ChangeType.Inserted)
                                {

                                    var linhaAntiga = mudancasComputadas.FirstOrDefault(l => l.Linha == linha.Text);
                                    if (linhaAntiga != null)
                                        linhaAntiga.Frequencia++;
                                    else
                                        mudancasComputadas.Add(new LinhaAlterada()
                                            {
                                                Linha = linha.Text,
                                                Frequencia = 1,
                                                Alteracao = linha.Type.ToString()
                                            });

                                }
                            }
                        }
                    }

                    #endregion

                    #region GA
                    if (rodadaGa !=null)
                    {
                        var diferencaTempoComUnload = (Decimal.Parse(rodadaGa.Fitness) / Decimal.Parse(rodadaGa.FitnessFinal) -1) *100;

                        string celulaTempoComUnload = "D" + (i + 2).ToString();
                        ws.Cells[celulaTempoComUnload].Style.Numberformat.Format = null;
                        ws.Cells[celulaTempoComUnload].Style.Numberformat.Format = "###,###,##0.00";
                        ws.Cells[celulaTempoComUnload].Value = diferencaTempoComUnload == 0 ? diferencaTempoComUnload : Decimal.Parse(diferencaTempoComUnload.ToString("#.##")); 

                        var diferencaLoc = rodadaGa.LocOriginal - rodadaGa.LocFinal;

                        string celulaLoc = "G" + (i + 2).ToString();
                        ws.Cells[celulaLoc].Style.Numberformat.Format = null;
                        ws.Cells[celulaLoc].Style.Numberformat.Format = "###,###,##0";
                        ws.Cells[celulaLoc].Value = diferencaLoc;

                        var diferencatokens = rodadaGa.CaracteresOriginal - rodadaGa.CaracteresFinal;

                        string celulaTokens = "J" + (i + 2).ToString();
                        ws.Cells[celulaTokens].Style.Numberformat.Format = null;
                        ws.Cells[celulaTokens].Style.Numberformat.Format = "###,###,##0";
                        ws.Cells[celulaTokens].Value = diferencatokens;

                        if (rodadaGa.Diferencas != null)
                        {
                            foreach (var linha in rodadaGa.Diferencas.Lines)
                            {
                                if (linha.Text.Trim().Equals(";"))
                                    continue;

                                if (linha.Type == ChangeType.Deleted || linha.Type == ChangeType.Modified || linha.Type == ChangeType.Inserted)
                                {

                                    var linhaAntiga = mudancasComputadas.FirstOrDefault(l => l.Linha == linha.Text);
                                    if (linhaAntiga != null)
                                        linhaAntiga.Frequencia++;
                                    else
                                        mudancasComputadas.Add(new LinhaAlterada()
                                        {
                                            Linha = linha.Text,
                                            Frequencia = 1,
                                            Alteracao = linha.Type.ToString()
                                        });

                                }
                            }
                        }
                    }

                    #endregion

                    #region Media e Desvio

                    ws.Cells["C34"].Style.Numberformat.Format = "###,###,##0.00";
                    ws.Cells["C34"].Formula = "=AVERAGE(C3:C32)";

                    ws.Cells["D34"].Style.Numberformat.Format = "###,###,##0.00";
                    ws.Cells["D34"].Formula = "=AVERAGE(D3:D32)";

                    ws.Cells["C35"].Style.Numberformat.Format = "###,###,##0.00";
                    ws.Cells["C35"].Formula = "=STDEV(C3:C32)";

                    ws.Cells["D35"].Style.Numberformat.Format = "###,###,##0.00";
                    ws.Cells["D35"].Formula = "=STDEV(D3:D32)";

                    ws.Cells["F34"].Style.Numberformat.Format = "###,###,##0.0";
                    ws.Cells["F34"].Formula = "=AVERAGE(F3:F32)";

                    ws.Cells["G34"].Style.Numberformat.Format = "###,###,##0.0";
                    ws.Cells["G34"].Formula = "=AVERAGE(G3:G32)";

                    ws.Cells["F35"].Style.Numberformat.Format = "###,###,##0.0";
                    ws.Cells["F35"].Formula = "=STDEV(F3:F32)";

                    ws.Cells["G35"].Style.Numberformat.Format = "###,###,##0.0";
                    ws.Cells["G35"].Formula = "=STDEV(G3:G32)";


                    ws.Cells["I34"].Style.Numberformat.Format = "###,###,##0.0";
                    ws.Cells["I34"].Formula = "=AVERAGE(I3:I32)";

                    ws.Cells["J34"].Style.Numberformat.Format = "###,###,##0.0";
                    ws.Cells["J34"].Formula = "=AVERAGE(J3:J32)";

                    ws.Cells["I35"].Style.Numberformat.Format = "###,###,##0.0";
                    ws.Cells["I35"].Formula = "=STDEV(I3:I32)";

                    ws.Cells["J35"].Style.Numberformat.Format = "###,###,##0.0";
                    ws.Cells["J35"].Formula = "=STDEV(J3:J32)";

                    #endregion

                    #region Mudanças computadas

                    var contador = 40;
                    
                    foreach (var mudanca in mudancasComputadas.OrderByDescending(m=> m.Frequencia))
                    {
                        string celulaAtual = "A" + contador.ToString();

                        ws.Cells[celulaAtual].Value = mudanca.Linha;

                        celulaAtual = "B" + contador.ToString();
                        
                        ws.Cells[celulaAtual].Value = mudanca.Alteracao;

                        celulaAtual = "C" + contador.ToString();

                        ws.Cells[celulaAtual].Value = mudanca.Frequencia;


                        contador++;
                    }


                    #endregion

                }

                ws.Cells.AutoFitColumns();
            }
            
            pck.Save();
        }

        /// <summary>
        /// Processa os arquivos em excel, conta os arquivos em Js e gera o relatório final
        /// </summary>
        /// <param name="fromDirectoryPath"></param>
        /// <param name="resultsDirectory"></param>
        private static void ProcessarDiretorios(string fromDirectoryPath, string resultsDirectory)
        {
            var dir = new DirectoryInfo(fromDirectoryPath);
            
            foreach (var subdir in dir.GetDirectories())
            {
                Console.WriteLine("     Processando : {0}", subdir.Name);

                var gaDir = subdir.GetDirectories().FirstOrDefault(n => n.Name == "GA");
                var hcDir = subdir.GetDirectories().FirstOrDefault(n => n.Name == "HC");
                var resultadoGa = new List<RodadaMapper>();
                var resultadoHc = new List<RodadaMapper>();

                if (gaDir != null)
                {
                    resultadoGa = ProcessarDiretorio(gaDir, subdir, resultsDirectory);
                }

                if (hcDir != null)
                {
                    resultadoHc = ProcessarDiretorio(hcDir, subdir, resultsDirectory);
                }

                var biblioteca = new BibliotecaMapper {Nome = subdir.Name};

                biblioteca.Rodadas.Add("GA", resultadoGa);
                biblioteca.Rodadas.Add("HC", resultadoHc);

                Bibliotecas.Add(biblioteca);
            }
        }

        /// <summary>
        /// Processa a roda do GA
        /// </summary>
        /// <param name="directoryGa"></param>
        /// <param name="biblioteca"></param>
        /// <param name="resultsDirectory"></param>
        private static List<RodadaMapper> ProcessarDiretorio(DirectoryInfo directoryGa, DirectoryInfo biblioteca, string resultsDirectory)
        {
            //pego o csv ou xsls
            var rodadas = new List<RodadaMapper>();
            var instanceFile = directoryGa.GetFiles().FirstOrDefault();

            if (instanceFile == null)
            {
                Console.WriteLine("Deveria existir um arquivo com as rodadas | {0}", directoryGa.FullName);
                Environment.Exit(-1);
            }

            Console.WriteLine("         Algoritmo {0}", directoryGa.Name);
            ConverterTodosExceis(directoryGa);

            if (instanceFile.Extension == ".csv")
            {
                rodadas = RecuperarRodadasDoGaNoCsv(instanceFile, biblioteca, directoryGa);
            }
            

            #region Exporta CSV

            var myExport = new CsvExport();

            foreach (var rodadaMapper in rodadas)
            {
                myExport.AddRow();

                myExport["Rodada"] = rodadaMapper.Rodada;
                myExport["TempoOriginalUnload"] = rodadaMapper.TempoOriginalComUnload;
                myExport["TempoFinalUnload"] = rodadaMapper.TempoFinalComUnload;
                myExport["TempoOriginalMS"] = rodadaMapper.Fitness;
                myExport["TempoFinalMS"] = rodadaMapper.FitnessFinal;
                myExport["LOCOriginal"] = rodadaMapper.LocOriginal;
                myExport["LOCFinal"] = rodadaMapper.LocFinal;
                myExport["CaracteresOrginal"] = rodadaMapper.CaracteresOriginal;
                myExport["CaracteresFinal"] = rodadaMapper.CaracteresFinal;
                myExport["Operacao"] = rodadaMapper.Operacao;
                myExport["Arquivo"] = rodadaMapper.Individuo;

            }

            var fileName = Path.Combine(resultsDirectory, biblioteca.Name + directoryGa.Name + ".csv");

            if (File.Exists(fileName))
                File.Delete(fileName);

            myExport.ExportToFile(fileName);

            #endregion

            return rodadas;
        }

        /// <summary>
        /// Encontra os arquivos em excel, converte em csv e os apaga
        /// </summary>
        /// <param name="directoryGa"></param>
        private static void ConverterTodosExceis(DirectoryInfo directoryGa)
        {
            var fileList = directoryGa.GetFiles("*.xlsx", SearchOption.AllDirectories);

            Console.WriteLine("             Convertendo {0} arquivos em csv.", fileList.Count());

            foreach (var fileInfo in fileList)
            {
                ConverterEmCsvEApagar(fileInfo);
            }
        }

        /// <summary>
        /// Converte o excel em csv e apaga o excel
        /// </summary>
        /// <param name="instanceFile"></param>
        private static void ConverterEmCsvEApagar(FileInfo instanceFile)
        {
            var myExport = new CsvExport();

            FileStream stream = File.Open(instanceFile.FullName, FileMode.Open, FileAccess.Read);

            // Reading from a OpenXml Excel file (2007 format; *.xlsx)
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            
            excelReader.IsFirstRowAsColumnNames = true;
            var firstRow = false;

            while (excelReader.Read())
            {
                if (firstRow)
                {
                    myExport.AddRow();

                    myExport["Rodada"] = excelReader.GetString(0);
                    myExport["Individuo"] = excelReader.GetString(1);
                    myExport["Operacao"] = excelReader.GetString(2);
                    myExport["Fitness"] = excelReader.GetString(3);
                    myExport["Tempo"] = excelReader.GetString(4).Replace("31/12/1899 ", ""); //Bug na leitura do campo 
                    myExport["Testes"] = excelReader.GetString(5);
                }

                firstRow = true;
            }
            
            // Free resources (IExcelDataReader is IDisposable)
            excelReader.Close();

            string output = instanceFile.FullName.Replace("xlsx", "csv");
            if (File.Exists(output))
                File.Delete(output);

            myExport.ExportToFile(output);

            File.Delete(instanceFile.FullName);

        }

        /// <summary>
        /// Lê o csv e retorna uma lista com as rodadas
        /// </summary>
        /// <param name="instanceFile"></param>
        /// <param name="biblioteca"></param>
        /// <param name="dirGa"></param>
        private static List<RodadaMapper> RecuperarRodadasDoGaNoCsv(FileInfo instanceFile, DirectoryInfo biblioteca, DirectoryInfo dirGa)
        {

            if (!File.Exists(biblioteca.GetFiles().First().FullName))
            {
                Console.WriteLine("Deveria existir o arquivo original da biblioteca aqui. | {0}", dirGa.FullName);
                Environment.Exit(-1);
            }


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
                //Texto do Original
                var textoOriginal = File.ReadAllText(biblioteca.GetFiles().First().FullName);


                while (!csv.EndOfData)
                {
                    string[] currentRow = csv.ReadFields();
                    string bestFile = Path.GetFileName(currentRow[1]);
                    int totalLinhasBest = 0;
                    int totalcharsBest = 0;
                    string tempoOriginal = "00:00:00,000";
                    string fitOrignal = "00000";
                    string tempoFinalComUnload = "";
                    string fitFinal = "";
                    DiffPaneModel resultadoComparacao = null;

                    //Tempo e fit originais
                    tempoOriginal = RecuperarTempoMedioeFitOriginal(biblioteca, dirGa, currentRow[0], out fitOrignal);


                    var fileList = dirGa.GetFiles(bestFile, SearchOption.AllDirectories);
                    //Se o arquivo melhorado existe, conta as linhas e os caracteres do mesmo

                    if (fileList.Any())
                    {
                        totalLinhasBest = ContarLinhas(fileList.First().FullName);
                        totalcharsBest = GetNumOfCharsInFile(fileList.First().FullName);
                        tempoFinalComUnload = currentRow[4];
                        fitFinal = currentRow[3];

                        var textoMelhor = File.ReadAllText(fileList.First().FullName);
                        
                        var d = new Differ();
                        var builder = new InlineDiffBuilder(d);
                        resultadoComparacao = builder.BuildDiffModel(textoOriginal, textoMelhor);
                    }
                    else
                    {
                        //Não houve melhor
                        totalLinhasBest = totalLinhas;
                        totalcharsBest = totalchars;
                        tempoFinalComUnload = tempoOriginal;
                        fitFinal = fitOrignal;
                    }


                    rodadas.Add(new RodadaMapper()
                        {
                            Algoritmo = dirGa.Name,
                            Biblioteca = biblioteca.Name,
                            Rodada = currentRow[0],
                            Individuo = currentRow[1],
                            Operacao = currentRow[2],
                            Fitness = fitOrignal,
                            FitnessFinal = fitFinal,
                            TempoOriginalComUnload = tempoOriginal,
                            TempoFinalComUnload = tempoFinalComUnload,
                            Testes = currentRow[5],
                            LocOriginal = totalLinhas,
                            LocFinal = totalLinhasBest,
                            CaracteresOriginal = totalchars,
                            CaracteresFinal = totalcharsBest,
                            Diferencas = resultadoComparacao
                        });
                }
            }

            return rodadas;
        }

        /// <summary>
        /// Ecnontra o csv da rodada em questão, pega o tempo médio do original e retorna
        /// </summary>
        /// <param name="biblioteca"></param>
        /// <param name="dirGa"></param>
        /// <param name="rodadaAlvo"></param>
        /// <param name="fitOrignal"></param>
        /// <returns></returns>
        private static string RecuperarTempoMedioeFitOriginal(DirectoryInfo biblioteca, DirectoryInfo dirGa, string rodadaAlvo, out string fitOrignal)
        {
            var fileList = dirGa.GetDirectories().First(d => d.Name.Equals(rodadaAlvo + "_Resultados" + biblioteca.Name)).GetFiles("resultados.csv", SearchOption.AllDirectories);
            var tempoOriginal = "0";
            fitOrignal = "0";

            var tempos = new List<TimeSpan>();
            var fits = new List<double>();

            if (fileList.Any())
            {
                using (var csv = new TextFieldParser(fileList.First().FullName))
                {
                    csv.ReadLine();
                    csv.ReadLine();

                    csv.TextFieldType = FieldType.Delimited;
                    csv.SetDelimiters(",");
                    csv.HasFieldsEnclosedInQuotes = true;
                    
                    //Geracao,Individuo,Operacao,Fitness,Tempo,Testes
                    //nesse arquivo as 5 primeiras linhas são o orignal
                    
                    for (int i = 0; i < 5; i++)
                    {
                        string[] currentRow = csv.ReadFields();

                        tempos.Add(TimeSpan.Parse(currentRow[4]));
                        fits.Add(Double.Parse(currentRow[3]));
                    }
                    
                        
                }
            }

            double mediaTempo = tempos.Average(t => t.Ticks);
            long longAverageTicks = Convert.ToInt64(mediaTempo);
            tempoOriginal = TimeSpan.FromTicks(longAverageTicks).ToString(@"hh\:mm\:ss\,ffff");

            fitOrignal = fits.Average().ToString();

            return tempoOriginal;
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
                    //Environment.Exit(-1); 
                }
            }
        }
    }
}
