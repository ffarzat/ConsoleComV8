using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        /// Configura o uso
        /// </summary>
        public void UsarSetTimeout()
        {
            _usarSetTimeout = true;
        }

        /// <summary>
        /// Executa a Otimização
        /// </summary>
        /// <param name="caminhoBibliotecaJs"></param>
        /// <param name="caminhoTestesJs"></param>
        /// <returns></returns>
        public bool Otimizar(string caminhoBibliotecaJs, string caminhoTestesJs)
        {
            _jHelper = new JavascriptHelper(_diretorioFontes, _usarSetTimeout, false);
            _caminhoScriptTestes = caminhoTestesJs;

            CriarPrimeiraGeracao(caminhoBibliotecaJs);
            return false;
        }

        /// <summary>
        /// Cria a primeira geração a partir do individuo base
        /// </summary>
        /// <param name="size"></param>
        /// <param name="jHelper"></param>
        /// <param name="caminhoBibliotecaJs"></param>
        /// <param name="caminhoTestesJs"></param>
        private void CriarPrimeiraGeracao(string caminhoBibliotecaJs)
        {
            CriarIndividuoOriginal(caminhoBibliotecaJs);

            _population.Add(_original);

            for (int i = 0; i < (_size -1); i++)
            {
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
            _original = new Individuo()
                {
                    Ast = _jHelper.GerarAst(File.ReadAllText(caminhoBibliotecaJs)),
                };

            _original.Arquivo = caminhoBibliotecaJs;
            _original.Codigo = _jHelper.GerarCodigo(_original.Ast);
            _original.Fitness = _jHelper.ExecutarTestes(caminhoBibliotecaJs, _caminhoScriptTestes);
        }

        /// <summary>
        /// Executa uma mutação no individuo
        /// </summary>
        /// <param name="sujeito"> </param>
        private void ExecutarMutacao(Individuo sujeito)
        {
            sujeito.Ast = _jHelper.ExecutarMutacaoExclusao(sujeito.Ast, Rand.Next());
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
            filhoPai = null;
            filhoMae = null;
        }

        /// <summary>
        /// Executa os testes com o sujeito e preenche sua propriedade Fitness Retorna o valor dessa propriedade
        /// </summary>
        /// <param name="sujeito"></param>
        private Int64 AvaliarIndividuo(Individuo sujeito)
        {
            var caminhoNovoAvaliado = GerarCodigo(sujeito);
            
            sujeito.Fitness = _jHelper.ExecutarTestes(caminhoNovoAvaliado, _caminhoScriptTestes);
            
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
            return caminhoNovoAvaliado;
        }
    }
}
