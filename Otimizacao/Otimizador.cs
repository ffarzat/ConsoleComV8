using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Otimizacao.Javascript;

namespace Otimizacao
{
    /// <summary>
    /// Representa um otimizador de javascript
    /// </summary>
    public class Otimizador: IDisposable
    {
        /// <summary>
        /// NLog Logger
        /// </summary>
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Timeout em segundos para avaliação de um individuo
        /// </summary>
        private int _timeout { get; set; }

        /// <summary>
        /// Indice global para individuos
        /// </summary>
        private int _countGlobal { get; set; }

        /// <summary>
        /// Tamanho da geração (Qtd de individuos)
        /// </summary>
        private int _size =100;

        /// <summary>
        /// Total de iterações
        /// </summary>
        private int _executarAte = 60;

        /// <summary>
        /// Contagem das gerações
        /// </summary>
        private int _generationCount = 0;

        // population parameters
        private double _crossOverRate = 0.75;
        private double _mutationRate = 0.25;

        // random number generator
        private static readonly Random Rand = new Random(int.MaxValue);

        //Controles totais
        private Int64 _fitnessMin = Int64.MaxValue;
        private Int64 _fitnessSum = 0;
        private double _fitnessAvg = 0;

        /// <summary>
        /// Configura o Uso de SetTimeout nos testes
        /// </summary>
        private bool _usarSetTimeout = false;

        /// <summary>
        /// Melhor individuo encontrado
        /// </summary>
        public Individuo MelhorIndividuo = null;

        /// <summary>
        /// Lista dos individuos atuais
        /// </summary>
        private List<Individuo> _population = new List<Individuo>();

        /// <summary>
        /// Diretorio de onde Ler os códigos em javascript
        /// </summary>
        private string _diretorioFontes;

        /// <summary>
        /// Onde salvar os arquivos das rodadas e relatórios de sáida
        /// </summary>
        private string _diretorioExecucao;

        /// <summary>
        /// Inidividuo original
        /// </summary>
        private Individuo _original;

        /// <summary>
        /// Caminho dos testes em Qunit
        /// </summary>
        private string _caminhoScriptTestes;

        /// <summary>
        /// Embaralhar a população após  a seleção?
        /// </summary>
        private bool _shuffle;

        /// <summary>
        /// Caminho da Biblioteca Js original
        /// </summary>
        private string _caminhoBiblioteca;

        /// <summary>
        /// Excel do relatório
        /// </summary>
        private ExcelPackage _excel;

        /// <summary>
        /// Usada na execução para o relatório
        /// </summary>
        private ExcelWorksheet Planilha { get { return _excel.Workbook.Worksheets["Resultados"]; } }

        /// <summary>
        /// Construtor Default
        /// </summary>
        public Otimizador(int tamanhoPopulacao, int totalGeracoes, int timeoutAvaliacaoIndividuo, string diretorioFontes, string diretorioExecucao)
        {
            _size = tamanhoPopulacao;
            _executarAte = totalGeracoes;
            _timeout = timeoutAvaliacaoIndividuo;
            _diretorioFontes = diretorioFontes;
            _diretorioExecucao = diretorioExecucao;
            _countGlobal = 0;

            LimparResultadosAnteriores();

            _excel = new ExcelPackage();
        }

        /// <summary>
        /// Limpa o diretório de resultados
        /// </summary>
        public void LimparResultadosAnteriores()
        {
            
//            if (File.Exists("ExecutionLog.txt"))
//                File.Delete("ExecutionLog.txt");


            if (Directory.Exists(_diretorioExecucao))
            {
                new DirectoryInfo(_diretorioExecucao).Delete(true);
            }

            Thread.Sleep(5);
            Directory.CreateDirectory(_diretorioExecucao);
            Thread.Sleep(5);
        }

        /// <summary>
        /// Configura o uso
        /// </summary>
        public void UsarSetTimeout()
        {
            _usarSetTimeout = true;
        }

        /// <summary>
        /// Executa a Otimização.
        /// </summary>
        /// <param name="caminhoBibliotecaJs"></param>
        /// <param name="caminhoTestesJs"></param>
        /// <returns>
        /// Verdadeiro se encontrar melhoria
        /// </returns>
        public bool Otimizar(string caminhoBibliotecaJs, string caminhoTestesJs)
        {
            var sw = new Stopwatch();
            sw.Start();

            _caminhoScriptTestes = caminhoTestesJs;
            _caminhoBiblioteca = caminhoBibliotecaJs;

            _logger.Info(string.Format("Iniciando Otimização do {0}", caminhoBibliotecaJs));
            _logger.Info(string.Format("    SetTimeout {0}", _usarSetTimeout));
            _logger.Info(string.Format("    Individuos {0}", _size));
            _logger.Info(string.Format("    Geracoes {0}", _executarAte));

            CriarExcel();
            
            CriarPrimeiraGeracao();

            ExecutarRodadas();
            
            sw.Stop();

            SalvarExcel();

            _logger.Info("Rodadas executadas com sucesso", _fitnessMin);
            
            
            var otimizou = MelhorIndividuo.Ast != _original.Ast;

            _logger.Info("============================================================");
            _logger.Info("Houve otimizacao: {0}", otimizou);

            _logger.Info("Tempo total: {0}", sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff"));

            return otimizou;
        }

        /// <summary>
        /// Cria a planilha em disco
        /// </summary>
        private void CriarExcel()
        {
            _excel.Workbook.Properties.Author = "Fabio Farzat";
            _excel.Workbook.Properties.Title = string.Format("Execucao do {0}", _caminhoBiblioteca);
            _excel.Workbook.Properties.Company = "www.vitalbusiness.com.br";

            _excel.Workbook.Worksheets.Add("Resultados");

            //Titulos
            //Planilha.Cells["A1:F1"].Style.Fill.PatternType = ExcelFillStyle.LightGrid;

            Planilha.Cells["A1"].Value = "Geracao";
            Planilha.Cells["B1"].Value = "Individuo";
            Planilha.Cells["C1"].Value = "Operacao";
            Planilha.Cells["D1"].Value = "Fitness";
            Planilha.Cells["E1"].Value = "Tempo";
            Planilha.Cells["F1"].Value = "Testes";

        }

        /// <summary>
        /// Salva o arquivo excel
        /// </summary>
        private void SalvarExcel()
        {
            
            if(File.Exists(Path.Combine(_diretorioExecucao, "resultados.xlsx")))
                File.Delete(Path.Combine(_diretorioExecucao, "resultados.xlsx"));

            var bin = _excel.GetAsByteArray();
            File.WriteAllBytes(Path.Combine(_diretorioExecucao, "resultados.xlsx"), bin);
        }

        /// <summary>
        /// Executa as rodadas configuradas
        /// </summary>
        private void ExecutarRodadas()
        {
            for (int i = 0; i < _executarAte; i++)
            {
                _generationCount = i;
                _logger.Info(string.Format("Geracao {0}", i));

                var sw = new Stopwatch();
                sw.Start();
                _logger.Info("      Executando cruzamentos...");

                Crossover();

                _logger.Info("      Executando mutacoes...");

                Mutate();

                ExecuteFitEvaluation();
                
                Selection();
                
                FindBestChromosomeOfRun();
                
                sw.Stop();

                _logger.Info("Geração avaliada em : {0}", sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff"));
                _logger.Info("===================================");
            }
            
        }

        /// <summary>
        /// Executa a seleção natural pelo método de Elite
        /// </summary>
        private void Selection()
        {
            var ordered = _population.OrderBy(c => c.Fitness).ToList();

            ordered.RemoveRange(_size, ordered.Count - _size);
            
            _population.Clear();
            _population.AddRange(ordered);

            if (_shuffle)
                EmbaralharPopulacao();
        }

        /// <summary>
        /// Embaralha a lista que antes estava ordenada pelo Fitness
        /// </summary>
        private void EmbaralharPopulacao()
        {
            var shuffled = _population.OrderBy(c => Rand.Next()).ToList();
            _population.Clear();
            _population.AddRange(shuffled);

        }

        /// <summary>
        /// Encerra uma geração
        /// </summary>
        private void FindBestChromosomeOfRun()
        {
            var melhores = new List<Individuo>();
            var originalValue = _fitnessMin;

            foreach (Individuo c in _population)
            {
                Int64 fitness = c.Fitness;

                _fitnessSum += fitness;

                // check for min
                if (fitness < _fitnessMin & fitness> 0)
                {
                    _fitnessMin = fitness;
                    MelhorIndividuo = c;
                    _logger.Info("-> Bom! Valor={0}", _fitnessMin);

                    melhores.Add(c);

                }
            }

            if (melhores.Count > 0)
            {
                #region Cria diretorio da geração
                string generationResultPath = Path.Combine(_diretorioExecucao, _generationCount.ToString());
                Directory.CreateDirectory(generationResultPath);
                Thread.Sleep(5);
                #endregion

                foreach (var individuo in melhores)
                {
                    string generationBestPath = string.Format("{0}\\{1}.js", generationResultPath, individuo.Id);

                    File.WriteAllText(generationBestPath, individuo.Codigo);
                }

            }

            _fitnessAvg = ((double) _fitnessSum / _size);
        }

        /// <summary>
        /// Gera uma página HTML com o diff entre o original e o novo melhor encontrado
        /// </summary>
        /// <param name="arquivo"></param>
        private void GerarRelatorioHtml(string arquivo)
        {
            
        }

        /// <summary>
        /// Avalia todos os individuos na geração
        /// </summary>
        private void ExecuteFitEvaluation()
        {
            var sujeitosParaAvaliar = _population.Where(ind => ind.Fitness == Int64.MaxValue).ToList();

            for (int i = 0; i < sujeitosParaAvaliar.Count(); i++)
            {
                var individuo = sujeitosParaAvaliar[i];
                AvaliarIndividuo(_countGlobal, individuo);
                _countGlobal++;
            }
        }

        /// <summary>
        /// Do crossover in the population
        /// </summary>
        private void Crossover()
        {

            var sw = new Stopwatch();
            sw.Start();

            int count = 0;

            for (int i = 1; i < _size; i += 2)
            {
                if (Rand.NextDouble() <= _crossOverRate)
                {
                    // clone both ancestors
                    Individuo c1 = _population[i - 1].Clone();
                    Individuo c2 = _population[i].Clone();

                    Individuo c3, c4;
                    ExecutarCruzamento(c1, c2, out c3, out c4);

                    // add two new offsprings to the population
                    _population.Add(c3);
                    _population.Add(c4);
                    
                    count++;
                }
            }
            sw.Stop();
            _logger.Info("      {0} crossover(s) executados em {1}", count, sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff"));
            
        }

        /// <summary>
        /// Do mutation in the population
        /// </summary>
        private void Mutate()
        {
            var sw = new Stopwatch();
            sw.Start();

            int count = 0;

            for (int i = 0; i < _size; i++)
            {
                // generate next random number and check if we need to do mutation

                if (Rand.NextDouble() <= _mutationRate)
                {
                    // clone the chromosome
                    Individuo c = _population[i].Clone();
                    ExecutarMutacao(c);
                    _population.Add(c);
                    count++;
                }
            }
            sw.Stop();
            _logger.Info("      {0} mutações executadas em {1}", count, sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff"));
        }

        /// <summary>
        /// Cria a primeira geração a partir do individuo base
        /// </summary>
        private void CriarPrimeiraGeracao()
        {
            CriarIndividuoOriginal(_caminhoBiblioteca);
            
            
            _logger.Info(string.Format("    Avaliando o original"));
            AvaliarIndividuo(0,_original);
            _fitnessMin = _original.Fitness;
            _population.Add(_original);

            _logger.Info(string.Format("    Criando a populaçao Inicial com {0} individuos",_size));
            
            for (int i = 1; i < (_size); i++) 
            {
                var atual = _original.Clone();
                ExecutarMutacao(atual);
                AvaliarIndividuo(i, atual);
                _population.Add(atual);
            }

            //vai que?
            FindBestChromosomeOfRun();

        }

        /// <summary>
        /// Popula o primeiro individuo
        /// </summary>
        /// <param name="caminhoBibliotecaJs"></param>
        private void CriarIndividuoOriginal(string caminhoBibliotecaJs)
        {
            var caminho = string.Format("{0}\\{1}", _diretorioFontes, caminhoBibliotecaJs);
            var caminhoDestino = string.Format("{0}\\{1}", _diretorioExecucao, caminhoBibliotecaJs);

            var jHelper = new JavascriptHelper(_diretorioFontes, _usarSetTimeout, false);
            jHelper.ConfigurarGeracao();
            
            var codigo = File.ReadAllText(caminho);
            var ast = jHelper.GerarAst(codigo);
            
            _original = new Individuo
                {
                    Ast = ast,
                    Arquivo = caminho,
                };

            
            _original.Codigo = jHelper.GerarCodigo(_original.Ast);
            File.WriteAllText(caminhoDestino, _original.Codigo);

            var sw = new Stopwatch();
            sw.Start();
            _original.Fitness = jHelper.ExecutarTestes(caminhoDestino, _caminhoScriptTestes);
            sw.Stop();
            _original.TempoExecucao = sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff");
            _original.TestesComSucesso = jHelper.TestesComSucesso;
            
            _fitnessMin = _original.Fitness;

            MelhorIndividuo = _original.Clone();

            jHelper.Dispose();
        }

        /// <summary>
        /// Executa uma mutação no individuo
        /// </summary>
        /// <param name="sujeito"> </param>
        [HandleProcessCorruptedStateExceptions]
        private void ExecutarMutacao(Individuo sujeito)
        {
            var jHelper = new JavascriptHelper(_diretorioFontes, _usarSetTimeout, false);
            jHelper.ConfigurarGeracao();
            
            int totalMutacoes = 1;
            string novaAst = "";

            while (novaAst == "" & totalMutacoes < 50)
            {
                if(totalMutacoes > 1)
                    _logger.Trace("          Tentativa {0} de executar mutação", totalMutacoes);

                var total = jHelper.ContarNos(sujeito.Ast);
                int no = Rand.Next(0, total);

                try
                {
                    novaAst = jHelper.ExecutarMutacaoExclusao(sujeito.Ast, no);
                }
                catch (Exception ex)
                {
                    _logger.Trace("          {0}", ex);
                }

                totalMutacoes++;
            }

            sujeito.Ast = novaAst;
            sujeito.CriadoPor = Operador.Mutacao;

            jHelper.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pai"></param>
        /// <param name="mae"></param>
        /// <param name="filhoPai"></param>
        /// <param name="filhoMae"></param>
        [HandleProcessCorruptedStateExceptions]
        private void ExecutarCruzamento(Individuo pai, Individuo mae, out Individuo filhoPai, out Individuo filhoMae)
        {

            var jHelper = new JavascriptHelper(_diretorioFontes, _usarSetTimeout, false);
            jHelper.ConfigurarGeracao();

            filhoPai = pai.Clone();
            filhoPai.CriadoPor= Operador.Cruzamento;
            
            filhoMae = mae.Clone();
            filhoMae.CriadoPor = Operador.Cruzamento;

            var totalPai = Rand.Next(0, jHelper.ContarNos(pai.Ast));
            var totalMae = Rand.Next(0, jHelper.ContarNos(mae.Ast));

            string c1 = "", c2 = "";
            try
            {
                jHelper.ExecutarCrossOver(pai.Ast, mae.Ast, totalPai, totalMae, out c1, out c2);
            }
            catch (Exception ex)
            {
                _logger.Info("          Erro ao executar cruzamento");
                _logger.Error(ex.ToString());
            }
            

            filhoPai.Ast = c1;
            filhoMae.Ast = c2;
            
            jHelper.Dispose();

        }

        /// <summary>
        /// Executa os testes com o sujeito e preenche sua propriedade Fitness Retorna o valor dessa propriedade
        /// </summary>
        /// <param name="indice"></param>
        /// <param name="sujeito"></param>
        [HandleProcessCorruptedStateExceptions]
        private Int64 AvaliarIndividuo(int indice, Individuo sujeito)
        {
            var sw = new Stopwatch();
            sw.Start();

            const long valorFitFalha = Int64.MaxValue - 100;

            var caminhoNovoAvaliado = GerarCodigo(sujeito);

            #region Codigo Vazio [sujeito inválido]

            if (string.IsNullOrEmpty(sujeito.Codigo))
            {
                _logger.Info("              Codigo Vazio");

                sujeito.Fitness = valorFitFalha;
                sujeito.TestesComSucesso = 0;
                sujeito.TempoExecucao = sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff");

                _logger.Info(string.Format("            FIT:{0}       | CTs: {1}            | T: {2}", sujeito.Fitness, sujeito.TestesComSucesso, sujeito.TempoExecucao));

                CriarLinhaExcel(indice, sujeito, sujeito.TestesComSucesso, sujeito.TempoExecucao);

                return sujeito.Fitness;
            }
            #endregion

            #region Igual ao Original
            if (_original.Codigo.Equals(sujeito.Codigo))
            {
                _logger.Info("              Igual ao Original");

                sujeito.TempoExecucao = _original.TempoExecucao;
                sujeito.TestesComSucesso = _original.TestesComSucesso;
                sujeito.Fitness = _original.Fitness;
                _logger.Info(string.Format("            FIT:{0}       | CTs: {1}            | T: {2}", sujeito.Fitness, sujeito.TestesComSucesso, sujeito.TempoExecucao));
                CriarLinhaExcel(indice, sujeito, sujeito.TestesComSucesso, sujeito.TempoExecucao);
                return sujeito.Fitness;
            }
            #endregion

            #region realmente executar os testes então

            var jHelper = new JavascriptHelper(_diretorioFontes, _usarSetTimeout, false);
            jHelper.ConfigurarTimeOut(_timeout);
            jHelper.ConfigurarMelhorFit(_fitnessMin);

            try
            {
                _logger.Trace("              Avaliando via testes");

                var avaliar = new Thread(() => sujeito.Fitness = jHelper.ExecutarTestes(caminhoNovoAvaliado, _caminhoScriptTestes));
                avaliar.Start();
                avaliar.Join(_timeout * 1000); //timeout

                sw.Stop();

                _logger.Info("              Executou até o final: {0}", jHelper.ExecutouTestesAteFinal);

                if (!jHelper.ExecutouTestesAteFinal)
                    sujeito.Fitness = valorFitFalha;

                if (jHelper.ExecutouTestesAteFinal && jHelper.TestesComFalha > 0)
                    sujeito.Fitness += jHelper.TestesComFalha;

                sujeito.TestesComSucesso = jHelper.TestesComSucesso;
                sujeito.TempoExecucao = sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff");

            }
            catch (Exception ex)
            {
                
                sujeito.Fitness = valorFitFalha;
                sujeito.TestesComSucesso = jHelper.TestesComSucesso;
                sujeito.TempoExecucao = sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff");

                _logger.Trace(ex);

                jHelper.Dispose();
            }

            #endregion

            _logger.Info(string.Format("            FIT:{0}       | CTs: {1}            | T: {2}", sujeito.Fitness, sujeito.TestesComSucesso, sujeito.TempoExecucao));

            CriarLinhaExcel(indice, sujeito, sujeito.TestesComSucesso, sujeito.TempoExecucao);

            jHelper.Dispose();

            return sujeito.Fitness;
        }

        /// <summary>
        /// Inclui a linha no excel
        /// </summary>
        /// <param name="indice"></param>
        /// <param name="sujeito"></param>
        /// <param name="testesComSucesso"></param>
        /// <param name="tempoTotal"></param>
        private void CriarLinhaExcel(int indice, Individuo sujeito, int testesComSucesso, string tempoTotal)
        {
            _logger.Info("              Incluído no excel : {0}", indice);

            var indiceExcel = indice + 2;

            Planilha.Cells["A" + indiceExcel].Value =_generationCount;
            Planilha.Cells["B" + indiceExcel].Value = sujeito.Arquivo;
            Planilha.Cells["C" + indiceExcel].Value = sujeito.CriadoPor.ToString();
            Planilha.Cells["D" + indiceExcel].Value = sujeito.Fitness;
            Planilha.Cells["E" + indiceExcel].Value = tempoTotal;
            Planilha.Cells["F" + indiceExcel].Value = testesComSucesso;
        }

        /// <summary>
        /// Gera o arquivo de código para avaliação
        /// </summary>
        /// <param name="sujeito"></param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        private string GerarCodigo(Individuo sujeito)
        {
            var jHelper = new JavascriptHelper(_diretorioFontes, _usarSetTimeout, false);
            jHelper.ConfigurarGeracao();

            try
            {
                sujeito.Codigo = jHelper.GerarCodigo(sujeito.Ast);
            }
            catch (Exception ex)
            {
                _logger.Trace(ex);
            }

            var caminhoNovoAvaliado = string.Format("{0}\\{1}.js", _diretorioExecucao, sujeito.Id);

            if (!string.IsNullOrEmpty(sujeito.Codigo))
            {
                File.WriteAllText(caminhoNovoAvaliado, sujeito.Codigo);
                sujeito.Arquivo = caminhoNovoAvaliado;
            }
            else
            {
                sujeito.Arquivo = "";
            }
            
            
            

            jHelper.Dispose();

            return caminhoNovoAvaliado;
        }

        /// <summary>
        /// Libera os objetos
        /// </summary>
        public void Dispose()
        {
            _excel.Dispose();
        }
    }
}
