using System;
using System.Collections.Generic;
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
        /// Helper para executar a otimização em Js
        /// </summary>
        private JavascriptHelper _jHelper;

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
        /// Construtor Default
        /// </summary>
        public Otimizador(int tamanhoPopulacao, int totalGeracoes, int timeoutAvaliacaoIndividuo, string diretorioFontes, string diretorioExecucao)
        {
            _size = tamanhoPopulacao;
            _executarAte = totalGeracoes;
            _timeout = timeoutAvaliacaoIndividuo;
            _diretorioFontes = diretorioFontes;
            _diretorioExecucao = diretorioExecucao;

            LimparResultadosAnteriores();
        }

        /// <summary>
        /// Limpa o diretório de resultados
        /// </summary>
        private void LimparResultadosAnteriores()
        {

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
            _jHelper = new JavascriptHelper(_diretorioFontes, _usarSetTimeout, false);
            _jHelper.ConfigurarGeracao();

            _caminhoScriptTestes = caminhoTestesJs;

            _jHelper.Log(string.Format("Iniciando Otimização do {0}", caminhoBibliotecaJs));
            _jHelper.Log(string.Format("    SetTimeout {0}", _usarSetTimeout));
            _jHelper.Log(string.Format("    Individuos {0}", _size));
            _jHelper.Log(string.Format("    Geracoes {0}", _executarAte));

            CriarPrimeiraGeracao(caminhoBibliotecaJs);

            ExecutarRodadas();


            return MelhorIndividuo.Ast == _original.Ast;
        }

        /// <summary>
        /// Executa as rodadas configuradas
        /// </summary>
        private void ExecutarRodadas()
        {
            for (int i = 0; i < _executarAte; i++)
            {
                _jHelper.Log(string.Format("Geracao {0}", i));
                Crossover();
                Mutate();
                ExecuteFitEvaluation();
                Selection();
                FindBestChromosomeOfRun();    
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
            foreach (Individuo c in _population)
            {
                Int64 fitness = c.Fitness;

                // accumulate summary value
                _fitnessSum += fitness;

                // check for min
                if (fitness < _fitnessMin)
                {
                    _fitnessMin = fitness;
                    MelhorIndividuo = c;
                    _logger.Info("============================== Achou melhor individuo novo! Valor={0} ==============================", _fitnessMin);
                }
            }
            _fitnessAvg = ((double) _fitnessSum / _size);
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

            _logger.Info("          {0} mutações executadas", count);
        }

        /// <summary>
        /// Cria a primeira geração a partir do individuo base
        /// </summary>
        /// <param name="caminhoBibliotecaJs"></param>
        private void CriarPrimeiraGeracao(string caminhoBibliotecaJs)
        {
            CriarIndividuoOriginal(caminhoBibliotecaJs);

            _population.Add(_original);

            _jHelper.Log(string.Format("    Criando a populaçao Inicial com {0} individuos",_size));
            
            for (int i = 0; i < (_size); i++) 
            {
                //_jHelper.Log(string.Format("    {0} - Fitness : {1}", i,  _original.Fitness));
                var atual = _original.Clone();
                ExecutarMutacao(atual);
                AvaliarIndividuo(atual);
                _population.Add(atual);
            }
        }

        /// <summary>
        /// Popula o primeiro individuo
        /// </summary>
        /// <param name="caminhoBibliotecaJs"></param>
        private void CriarIndividuoOriginal(string caminhoBibliotecaJs)
        {
            var caminho = string.Format("{0}\\{1}", _diretorioFontes, caminhoBibliotecaJs);

            _original = new Individuo
                {
                    Ast = _jHelper.GerarAst(File.ReadAllText(caminho)),
                    Arquivo = caminho,
                };

            _original.Codigo = _jHelper.GerarCodigo(_original.Ast);
            _original.Fitness = _jHelper.ExecutarTestes(caminhoBibliotecaJs, _caminhoScriptTestes);

            _jHelper.Log(string.Format("    Fitness do Original {0}", _original.Fitness));
        }

        /// <summary>
        /// Executa uma mutação no individuo
        /// </summary>
        /// <param name="sujeito"> </param>
        private void ExecutarMutacao(Individuo sujeito)
        {
            var total = _jHelper.ContarNos(sujeito.Ast);
            int no = Rand.Next(0, total);
            
            if(total > 0 )
                sujeito.Ast = _jHelper.ExecutarMutacaoExclusao(sujeito.Ast, no);
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
            filhoPai = pai.Clone();
            filhoMae = mae.Clone();
            var totalPai = _jHelper.ContarNos(pai.Ast);
            var totalMae = _jHelper.ContarNos(mae.Ast);

            string c1, c2;
            try
            {
                _jHelper.ExecutarCrossOver(pai.Ast, mae.Ast, totalPai, totalMae, out c1, out c2);
            }
            catch (Exception ex)
            {
                _jHelper.Log(ex.ToString());
                return;
            }
            

            filhoPai.Ast = c1;
            filhoMae.Ast = c2;
        }

        /// <summary>
        /// Executa os testes com o sujeito e preenche sua propriedade Fitness Retorna o valor dessa propriedade
        /// </summary>
        /// <param name="sujeito"></param>
        private Int64 AvaliarIndividuo(Individuo sujeito)
        {
            var caminhoNovoAvaliado = GerarCodigo(sujeito);

            //_jHelper.Log(string.Format("            Avaliando {0}", sujeito.Arquivo));
            sujeito.Fitness = _jHelper.ExecutarTestes(caminhoNovoAvaliado, _caminhoScriptTestes);
            _jHelper.Log(string.Format("            {0}", sujeito.Fitness));

            return sujeito.Fitness;
        }

        /// <summary>
        /// Gera o arquivo de código para avaliação
        /// </summary>
        /// <param name="sujeito"></param>
        /// <returns></returns>
        private string GerarCodigo(Individuo sujeito)
        {
            sujeito.Codigo = _jHelper.GerarCodigo(sujeito.Ast);

            var caminhoNovoAvaliado = string.Format("{0}\\{1}.js", _diretorioExecucao, Guid.NewGuid());
            File.WriteAllText(caminhoNovoAvaliado, sujeito.Codigo);
            
            sujeito.Arquivo = caminhoNovoAvaliado;

            return caminhoNovoAvaliado;
        }
    }
}
