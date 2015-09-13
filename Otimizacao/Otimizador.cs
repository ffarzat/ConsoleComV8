using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Otimizacao.Javascript;

namespace Otimizacao
{
    /// <summary>
    /// Representa um otimizador de javascript
    /// </summary>
    public class Otimizador
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
        private double _mutationRate = 0.10;

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
        /// Construtor Default
        /// </summary>
        public Otimizador(int tamanhoPopulacao, int totalGeracoes, int timeoutAvaliacaoIndividuo, string diretorioFontes, string diretorioExecucao)
        {
            _size = tamanhoPopulacao;
            _executarAte = totalGeracoes;
            _timeout = timeoutAvaliacaoIndividuo;
            _diretorioFontes = diretorioFontes;
            _diretorioExecucao = diretorioExecucao;
        }

        /// <summary>
        /// Limpa o diretório de resultados
        /// </summary>
        public void LimparResultadosAnteriores()
        {
            
            if (File.Exists("ExecutionLog.txt"))
                File.Delete("ExecutionLog.txt");


            if (Directory.Exists(_diretorioExecucao))
            {
                new DirectoryInfo(_diretorioExecucao).Delete(true);
            }

            Thread.Sleep(10);

            Directory.CreateDirectory(_diretorioExecucao);
            
            Thread.Sleep(10);
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

            CriarPrimeiraGeracao();

            ExecutarRodadas();
            
            sw.Stop();

            _logger.Info("Rodadas executadas com sucesso", _fitnessMin);
            
            
            var otimizou = MelhorIndividuo.Ast != _original.Ast;

            _logger.Info("============================================================");
            _logger.Info("Houve otimizacao: {0}", otimizou);

            _logger.Info("Tempo total: {0}", sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff"));

            return otimizou;
        }

        /// <summary>
        /// Executa as rodadas configuradas
        /// </summary>
        private void ExecutarRodadas()
        {
           

            for (int i = 0; i < _executarAte; i++)
            {
                var sw = new Stopwatch();
                sw.Start();
                _logger.Info(string.Format("Geracao {0}", i));
                Crossover();
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
            var originalValue = _fitnessMin;

            foreach (Individuo c in _population)
            {
                Int64 fitness = c.Fitness;

                _fitnessSum += fitness;

                // check for min
                if (fitness < _fitnessMin)
                {
                    _fitnessMin = fitness;
                    MelhorIndividuo = c;
                }
            }

            if (originalValue != _fitnessMin)
            {
                _logger.Info("-> Achou melhor individuo novo! Valor={0}", _fitnessMin);
                _logger.Info("-> Criado por = {0}", MelhorIndividuo.CriadoPor.ToString());
                _logger.Info("-> Arquivo = {0}", MelhorIndividuo.Arquivo);

                string generationResultPath = Path.Combine(_diretorioExecucao, _generationCount.ToString());
                string generationBestPath = Path.Combine(_diretorioExecucao, "\\melhor.js");
                
                Directory.CreateDirectory(generationResultPath);
                File.WriteAllText(generationBestPath, MelhorIndividuo.Codigo);

                GerarRelatorioHtml(generationBestPath);
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
            foreach (var individuo in _population)
            {
                if (individuo.Fitness == Int64.MaxValue)
                    AvaliarIndividuo(individuo);
            }

        }

        /// <summary>
        /// Do crossover in the population
        /// </summary>
        private void Crossover()
        {
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

            _logger.Info("      {0} crossover(s) executados", count);

        }

        /// <summary>
        /// Do mutation in the population
        /// </summary>
        private void Mutate()
        {
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

            _logger.Info("      {0} mutações executadas", count);
        }

        /// <summary>
        /// Cria a primeira geração a partir do individuo base
        /// </summary>
        private void CriarPrimeiraGeracao()
        {
            CriarIndividuoOriginal(_caminhoBiblioteca);
            _population.Add(_original);
            
            _logger.Info(string.Format("    Avaliando o original"));
            AvaliarIndividuo(_original);
            _fitnessMin = _original.Fitness;

            _logger.Info(string.Format("    Criando a populaçao Inicial com {0} individuos",_size));
            
            for (int i = 0; i < (_size); i++) 
            {
                var atual = _original.Clone();
                ExecutarMutacao(atual);
                AvaliarIndividuo(atual);
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
            
            _original.Fitness = jHelper.ExecutarTestes(caminhoDestino, _caminhoScriptTestes);
            
            _fitnessMin = _original.Fitness;

            MelhorIndividuo = _original.Clone();

            jHelper.Dispose();
        }

        /// <summary>
        /// Executa uma mutação no individuo
        /// </summary>
        /// <param name="sujeito"> </param>
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
                novaAst = jHelper.ExecutarMutacaoExclusao(sujeito.Ast, no);
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
        private void ExecutarCruzamento(Individuo pai, Individuo mae, out Individuo filhoPai, out Individuo filhoMae)
        {
            var jHelper = new JavascriptHelper(_diretorioFontes, _usarSetTimeout, false);
            jHelper.ConfigurarGeracao();

            filhoPai = pai.Clone();
            filhoPai.CriadoPor= Operador.Cruzamento;
            
            filhoMae = mae.Clone();
            filhoMae.CriadoPor = Operador.Cruzamento;

            var totalPai = jHelper.ContarNos(pai.Ast);
            var totalMae = jHelper.ContarNos(mae.Ast);

            string c1, c2;
            try
            {
                jHelper.ExecutarCrossOver(pai.Ast, mae.Ast, totalPai, totalMae, out c1, out c2);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                return;
            }
            

            filhoPai.Ast = c1;
            filhoMae.Ast = c2;
            
            jHelper.Dispose();

        }

        /// <summary>
        /// Executa os testes com o sujeito e preenche sua propriedade Fitness Retorna o valor dessa propriedade
        /// </summary>
        /// <param name="sujeito"></param>
        private Int64 AvaliarIndividuo(Individuo sujeito)
        {
            var sw = new Stopwatch();
            sw.Start();

            var jHelper = new JavascriptHelper(_diretorioFontes, _usarSetTimeout, false);
            jHelper.ConfigurarTimeOut(_timeout);
            jHelper.ConfigurarMelhorFit(_fitnessMin);
            var caminhoNovoAvaliado = GerarCodigo(sujeito);
            
            sujeito.Fitness = jHelper.ExecutarTestes(caminhoNovoAvaliado, _caminhoScriptTestes);
            sw.Stop();
            
            _logger.Info(string.Format("            FIT:{0}       | CTs: {1}            | T: {2}", sujeito.Fitness, jHelper.TestesComSucesso, sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff")));

            jHelper.Dispose();

            //Não deveria nunca acontecer de sujeitos iguais
            if (_original.Ast == sujeito.Ast)
                sujeito.Fitness = _original.Fitness;

            return sujeito.Fitness;
        }

        /// <summary>
        /// Gera o arquivo de código para avaliação
        /// </summary>
        /// <param name="sujeito"></param>
        /// <returns></returns>
        private string GerarCodigo(Individuo sujeito)
        {
            var jHelper = new JavascriptHelper(_diretorioFontes, _usarSetTimeout, false);
            jHelper.ConfigurarGeracao();

            sujeito.Codigo = jHelper.GerarCodigo(sujeito.Ast);

            var caminhoNovoAvaliado = string.Format("{0}\\{1}.js", _diretorioExecucao, sujeito.Id);
            File.WriteAllText(caminhoNovoAvaliado, sujeito.Codigo);
            
            sujeito.Arquivo = caminhoNovoAvaliado;

            jHelper.Dispose();

            return caminhoNovoAvaliado;
        }
    }
}
