using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Newtonsoft.Json.Linq;
using Otimizacao.Javascript;

namespace Otimizacao
{
    /// <summary>
    /// Representa um otimizador de javascript
    /// </summary>
    public class Otimizador: IDisposable
    {
        /// <summary>
        /// V8Engine Helper
        /// </summary>
        private JavascriptHelper _javascriptHelper;

        //Guarda o indice dos nós por tipo
        private static List<No> _nosParaMutacao = new List<No>();

        /// <summary>
        /// Guarda a AST das funções da biblioteca
        /// </summary>
        private static Dictionary<string, string> _astDasFuncoes = new Dictionary<string, string>(); 

        /// <summary>
        /// Guarda qual das rodadas externas é a atual
        /// </summary>
        public int RodadaGlobalExterna { get; set; }

        /// <summary>
        /// GA ou HC por enquanto
        /// </summary>
        public string Heuristica { get; set; }

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
        private double _fitnessMin = double.MaxValue;
        private double _fitnessSum = 0;
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
        /// Total de nós
        /// </summary>
        private int _total;

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

            Heuristica = "GA";

            LimparResultadosAnteriores();
        }

        /// <summary>
        /// Limpa o diretório de resultados
        /// </summary>
        public void LimparResultadosAnteriores()
        {
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

            bool otimizou = false;

            _caminhoScriptTestes = caminhoTestesJs;
            _caminhoBiblioteca = caminhoBibliotecaJs;

            Console.WriteLine(string.Format("Iniciando Otimização do {0}", caminhoBibliotecaJs));
            Console.WriteLine(string.Format("    SetTimeout {0}", _usarSetTimeout));
            Console.WriteLine(string.Format("    Heuristica {0}", Heuristica));

            _javascriptHelper = new JavascriptHelper(_diretorioFontes, _usarSetTimeout, false);
            _javascriptHelper.ConfigurarGeracao();

            var sw = new Stopwatch();
            sw.Start();
            if(Heuristica == "GA")
                otimizou = OtimizarUsandoGa();
            else if(Heuristica == "RD")
                otimizou = OtimizarUsandoRd();
            else if (Heuristica == "HC")
                otimizou = OtimizarUsandoHc();
            else if (Heuristica == "HCF")
                otimizou = OtimizarUsandoHcPorFuncao();
            else
                throw new ApplicationException(string.Format("Heurística ainda não definida. {0}", Heuristica));
                
            #region Gera o CSV da rodada

            var myExport = new CsvExport();

            myExport.AddRow();
            myExport["Rodada"] = RodadaGlobalExterna;
            myExport["Individuo"] = MelhorIndividuo.Arquivo;
            myExport["Operacao"] = MelhorIndividuo.CriadoPor;
            myExport["Fitness"] = MelhorIndividuo.Fitness;
            myExport["Tempo"] = MelhorIndividuo.TempoExecucao;
            myExport["Testes"] = MelhorIndividuo.TestesComSucesso;

            myExport.ExportToFile("rodadas.csv");

            #endregion

            #region limpa o diretório de execução.

            var files = new DirectoryInfo(_diretorioExecucao).EnumerateFiles("*.js").ToList();

            files.ForEach(f => f.Delete());

            #endregion

            sw.Stop();
            Console.WriteLine("  Tempo total: {0}", sw.Elapsed.ToString(@"hh\:mm\:ss\,ffff"));

            return otimizou;
        }

        /// <summary>
        /// Escopo do HC por função
        /// </summary>
        /// <returns></returns>
        private bool OtimizarUsandoHcPorFuncao()
        {
            var totalVizinhosExplorar = _size * _executarAte;
            var otimizado = false;
            var melhores = new List<Individuo>();
            Console.WriteLine("      Avaliar {0} vizinhos", totalVizinhosExplorar);

            CriarIndividuoOriginal(_caminhoBiblioteca);
            AvaliarIndividuo(0, MelhorIndividuo);
            _fitnessMin = MelhorIndividuo.Fitness;

            var funcoesOtimizar = DeterminarListaDeFuncoes(MelhorIndividuo.Clone());
            int indiceFuncaoAtual = 0;
            //IfStatement
            //CallExpression

            var funcaoEmOtimizacao = funcoesOtimizar[indiceFuncaoAtual];
            var r = new Random();
            //var totalNos = CalcularTodosVizinhos(funcaoEmOtimizacao.Ast);
            CalcularVizinhos(funcaoEmOtimizacao.Ast);// Atualiza a propriedade _nosParaMutacao
            int control = 0;
            
            Console.WriteLine("     {0} é utlizada {1}x", funcaoEmOtimizacao.Nome, funcaoEmOtimizacao.Total);
            Console.WriteLine("     {0} vizinhos para avaliar", _nosParaMutacao.Count);
            
            //Explorando os vizinhos
            for (int i = 0; i < totalVizinhosExplorar - 1; i++)
            {
                if (_nosParaMutacao.Count > 0)
                {

                    #region cria o vizinho

                    Console.WriteLine("      {0}|Nó:{1}|{2}", i, control, _nosParaMutacao[control].Tipo);

                    Individuo c = MelhorIndividuo.Clone(); //Sempre usando o melhor

                    //var novaFuncao = ExecutarMutacaoNaFuncao(funcaoEmOtimizacao.Ast, control);
                    var novaFuncao = ExecutarMutacaoNaFuncao(funcaoEmOtimizacao.Ast, _nosParaMutacao[control].Indice);

                    c.Ast = AtualizarFuncao(c, funcaoEmOtimizacao.Nome, novaFuncao);
                    c.CriadoPor = Operador.Mutacao;

                    #endregion

                    #region Avalia o vizinho

                    var fitvizinho = AvaliarIndividuo(i, c);

                    if (fitvizinho < 0)
                        fitvizinho = fitvizinho*-1;


                    if (fitvizinho < _fitnessMin)
                    {
                        Console.WriteLine("      Encontrado. FIT Antigo {0} | FIT novo {1}", _fitnessMin, c.Fitness);
                        MelhorIndividuo = c;
                        _fitnessMin = fitvizinho;
                        otimizado = true;
                        melhores.Add(c);

                        funcaoEmOtimizacao.Ast = novaFuncao; //Atualizo a melhor nova função

                        control = 0;

                        //CalcularVizinhos(ast); //recalculo os nós
                    }

                    #endregion

                    control++;
                }

                #region Critérios de parada
                //Queimou o Orçamento global 
                if (i == totalVizinhosExplorar )
                {
                    break;
                }
                
                //acabaram os nós na função atual?
                if (control == _nosParaMutacao.Count )
                {
                    indiceFuncaoAtual++;
                    funcaoEmOtimizacao = funcoesOtimizar[indiceFuncaoAtual];
                    //totalNos = CalcularTodosVizinhos(funcaoEmOtimizacao.Ast);
                    CalcularVizinhos(funcaoEmOtimizacao.Ast);
                    control = 0;
                    Console.WriteLine("     {0} é utlizada {1}x", funcaoEmOtimizacao.Nome, funcaoEmOtimizacao.Total);
                    Console.WriteLine("     {0} vizinhos para avaliar", _nosParaMutacao.Count);
                }

                #endregion

            }

            #region Cria diretorio dos resultados
            string generationResultPath = Path.Combine(_diretorioExecucao, "0");
            Directory.CreateDirectory(generationResultPath);
            Thread.Sleep(5);
            #endregion

            foreach (var individuo in melhores)
            {
                string generationBestPath = string.Format("{0}\\{1}.js", generationResultPath, individuo.Id);

                File.WriteAllText(generationBestPath, individuo.Codigo);
            }


            Console.WriteLine("============================================================");
            Console.WriteLine("  Houve otimizacao: {0}", otimizado);

            return otimizado;

        }

        /// <summary>
        /// Calcular todos os vizinhos
        /// </summary>
        /// <param name="ast"></param>
        private int CalcularTodosVizinhos(string ast)
        {
            return _javascriptHelper.ContarNos(ast);
        }

        /// <summary>
        /// Otimizar usando um HC que salta os vizinhos de IF e CALL
        /// </summary>
        /// <returns></returns>
        private bool OtimizarUsandoHc()
        {
            var totalVizinhosExplorar = _size * _executarAte;
            var otimizado = false;
            var melhores = new List<Individuo>();

            CriarIndividuoOriginal(_caminhoBiblioteca);

            CalcularVizinhos(_original.Ast);

            Console.WriteLine("      {0} nós para remover (IF, CALL).  ", _nosParaMutacao.Count);

            Console.WriteLine("      Avaliar {0} vizinhos", totalVizinhosExplorar);

            AvaliarIndividuo(0, MelhorIndividuo);

            //IfStatement
            //CallExpression

            var r = new Random();

            int ultimoIndice = r.Next(0, _nosParaMutacao.Count);

            for (int i = 1; i < totalVizinhosExplorar - 1; i++)
            {

                if (_nosParaMutacao.Count <= ultimoIndice) //zera de novo
                    ultimoIndice = 0;

                var no = _nosParaMutacao[ultimoIndice];

                #region cria o vizinho
                Console.WriteLine("      {0}|Nó:{1}|{2}", i, ultimoIndice, no.Tipo);
                Individuo c = MelhorIndividuo.Clone(); //Sempre usando o melhor

                ExecutarMutacao(c, no.Indice);
                #endregion

                ultimoIndice++;

                //Avalia o vizinho e veja se melhorou
                var fitvizinho = AvaliarIndividuo(i, c);

                if (fitvizinho < 0)
                    fitvizinho = fitvizinho * -1;


                if (fitvizinho < _fitnessMin)
                {
                    Console.WriteLine("      Encontrado. FIT Antigo {0} | FIT novo {1}", _fitnessMin, c.Fitness);
                    MelhorIndividuo = c;
                    _fitnessMin = fitvizinho;
                    otimizado = true;
                    melhores.Add(c);

                    CalcularVizinhos(MelhorIndividuo.Ast); //recalculo os nós
                }

                if (_nosParaMutacao.Count == i) //se deu a volta completa pode parar
                    break;

            }

            #region Cria diretorio dos resultados
            string generationResultPath = Path.Combine(_diretorioExecucao, "0");
            Directory.CreateDirectory(generationResultPath);
            Thread.Sleep(5);
            #endregion

            foreach (var individuo in melhores)
            {
                string generationBestPath = string.Format("{0}\\{1}.js", generationResultPath, individuo.Id);

                File.WriteAllText(generationBestPath, individuo.Codigo);
            }


            Console.WriteLine("============================================================");
            Console.WriteLine("  Houve otimizacao: {0}", otimizado);

            return otimizado;
        }

        /// <summary>
        /// Calculo o número de vizinhos
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        private void CalcularVizinhos(string ast)
        {
            //IfStatement
            //CallExpression
            var lista = new List<string>()
                {
                    {"IfStatement"},
                    {"CallExpression"}
                };


            _nosParaMutacao.Clear();
            _nosParaMutacao = new List<No>();
            _nosParaMutacao = _javascriptHelper.ContarNosPorTipo(ast, lista);
            
        }

        /// <summary>
        /// Lista de Funcoes com os detalhes para otimizaçao
        /// </summary>
        /// <returns></returns>
        public List<Function> DeterminarListaDeFuncoes(Individuo clone)
        {
            var nos = _javascriptHelper.ContarNosCallee(clone.Ast);

            var funcoesEncontradas = nos.GroupBy(f => f.NomeFuncao).Select(n => new Function { Nome = n.Key, Total = n.Count(), Ast = ""}).OrderByDescending(n => n.Total).ToList();

            _astDasFuncoes = ProcessarAstDasFuncoes(funcoesEncontradas, clone);

            foreach (var funcaoEncontrada in funcoesEncontradas)
            {
                //funcaoEncontrada.Ast = 
                funcaoEncontrada.Ast =  _astDasFuncoes.ContainsKey(funcaoEncontrada.Nome) ? _astDasFuncoes[funcaoEncontrada.Nome] : "";
            }
            
            return funcoesEncontradas;
        }

        /// <summary>
        /// Percorre a árvore e junta todos os functions declarations
        /// </summary>
        /// <param name="funcoesEncontradas"></param>
        /// <param name="biblioteca"></param>
        /// <returns></returns>
        private Dictionary<string, string> ProcessarAstDasFuncoes(IEnumerable<Function> funcoesEncontradas, Individuo biblioteca)
        {

            var listaDeNomes = funcoesEncontradas.Select(n => n.Nome).ToList();

            var dicionarioResultado = _javascriptHelper.RecuperarTodasAstDeFuncao(biblioteca.Ast, listaDeNomes);

            return dicionarioResultado;
        }

        /// <summary>
        /// Troca a função antiga pela ASTnova
        /// </summary>
        /// <returns></returns>
        public string AtualizarFuncao(Individuo clone, string nomeFuncao, string astFuncaoNova)
        {
            var astFuncao = _javascriptHelper.AtualizarDeclaracaoFuncaoPeloNome(clone.Ast, nomeFuncao, astFuncaoNova);

            return astFuncao;
        }


        /// <summary>
        /// Usar Ramdon para otimizar
        /// </summary>
        /// <returns></returns>
        private bool OtimizarUsandoRd()
        {
            var totalVizinhosExplorar = _size * _executarAte;
            var moverNoPrimeiroMelhor = true;
            var otimizado = false;
            var melhores = new List<Individuo>();

            Console.WriteLine("      Avaliar {0} vizinhos", totalVizinhosExplorar);

            CriarIndividuoOriginal(_caminhoBiblioteca);

            AvaliarIndividuo(0, MelhorIndividuo);

            
            for (int i = 1; i < totalVizinhosExplorar -1; i++)
            {
                //cria o vizinho
                Console.WriteLine("      {0}", i);
                
                Individuo c = MelhorIndividuo.Clone();
                
                ExecutarMutacao(c); 
                
                //Avalia o vizinho e veja se melhorou
                var fitvizinho = AvaliarIndividuo(i, c);

                if (fitvizinho < 0)
                    fitvizinho = fitvizinho*-1;


                if (fitvizinho < _fitnessMin)
                {
                    Console.WriteLine("      Encontrado. FIT Antigo {0} | FIT novo {1}", _fitnessMin, c.Fitness);
                    MelhorIndividuo = c;
                    _fitnessMin = fitvizinho;
                    otimizado = true;
                    melhores.Add(c);
                }
            }

            #region Cria diretorio dos resultados
            string generationResultPath = Path.Combine(_diretorioExecucao, "0");
            Directory.CreateDirectory(generationResultPath);
            Thread.Sleep(5);
            #endregion

            foreach (var individuo in melhores)
            {
                string generationBestPath = string.Format("{0}\\{1}.js", generationResultPath, individuo.Id);

                File.WriteAllText(generationBestPath, individuo.Codigo);
            }


            Console.WriteLine("============================================================");
            Console.WriteLine("  Houve otimizacao: {0}", otimizado);

            return otimizado;
        }

        /// <summary>
        /// Usar GA como Heuristica de busca
        /// </summary>
        /// <returns></returns>
        private bool OtimizarUsandoGa()
        {

            Console.WriteLine(string.Format("    Individuos {0}", _size));
            Console.WriteLine(string.Format("    Geracoes {0}", _executarAte));

            CriarPrimeiraGeracao();

            ExecutarRodadas();

            

            Console.WriteLine("Rodada {0} executada com sucesso", RodadaGlobalExterna);

            var otimizou = MelhorIndividuo.Ast != _original.Ast;

            Console.WriteLine("============================================================");
            Console.WriteLine("  Houve otimizacao: {0}", otimizou);

            return otimizou;
        }

        /// <summary>
        /// Executa as rodadas configuradas
        /// </summary>
        private void ExecutarRodadas()
        {
            for (int i = 0; i < _executarAte; i++)
            {
                _generationCount = i;
                Console.WriteLine(string.Format("Geracao {0}", i));

                var sw = new Stopwatch();
                sw.Start();
                Console.WriteLine("      Executando cruzamentos...");

                Crossover();

                Console.WriteLine("      Executando mutacoes...");

                Mutate();

                Console.WriteLine("      Avaliando...");

                ExecuteFitEvaluation();

                Console.WriteLine("      Selecionando...");

                Selection();
                
                FindBestChromosomeOfRun();
                
                sw.Stop();

                Console.WriteLine("Geração avaliada em : {0}", sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff"));
                Console.WriteLine("===================================");
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
                double fitness = c.Fitness;

                _fitnessSum += fitness;

                // check for min
                if (fitness < _fitnessMin & fitness> 0)
                {
                    _fitnessMin = fitness;
                    MelhorIndividuo = c;
                    Console.WriteLine("-> Bom! Valor={0}", _fitnessMin);

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


                if (_generationCount > 0)
                {
                    var di = new DirectoryInfo(_diretorioExecucao);
                    di.EnumerateFiles().Where(n => !n.Name.Contains("resultados")).ToList().ForEach(a => a.Delete());    
                }
                

            }

            _fitnessAvg = ((double) _fitnessSum / _size);
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
            Console.WriteLine("      {0} crossover(s) executados em {1}", count, sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff"));
            
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
            Console.WriteLine("      {0} mutações executadas em {1}", count, sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff"));
        }

        /// <summary>
        /// Cria a primeira geração a partir do individuo base
        /// </summary>
        private void CriarPrimeiraGeracao()
        {
            CriarIndividuoOriginal(_caminhoBiblioteca);
            
            
            Console.WriteLine(string.Format("    Avaliando o original"));
            AvaliarIndividuo(0,_original);
            _fitnessMin = _original.Fitness;
            _population.Add(_original);

            Console.WriteLine(string.Format("    Criando a populaçao Inicial com {0} individuos",_size));
            
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
        [HandleProcessCorruptedStateExceptions]
        private void CriarIndividuoOriginal(string caminhoBibliotecaJs)
        {
            var caminho = string.Format("{0}\\{1}", _diretorioFontes, caminhoBibliotecaJs);
            var caminhoDestino = string.Format("{0}\\{1}", _diretorioExecucao, caminhoBibliotecaJs);

            _original = new Individuo
            {
                Arquivo = caminho,
            };

            int contador = 0;

            while (_original.Fitness == Int64.MaxValue & contador < 50)
            {

                Console.WriteLine("      Criando e V8engine - Tentativa {0}", contador);

                try
                {
                    

                    var codigo = File.ReadAllText(caminho);
                    var ast = _javascriptHelper.GerarAst(codigo);

                    _original.Ast = ast;


                    _original.Codigo = _javascriptHelper.GerarCodigo(_original.Ast);
                    File.WriteAllText(caminhoDestino, _original.Codigo);

                    _total = _javascriptHelper.ContarNos(_original.Ast);

                    var sw = new Stopwatch();
                    sw.Start();
                    _original.Fitness = _javascriptHelper.ExecutarTestes(caminhoDestino, _caminhoScriptTestes);
                    sw.Stop();
                    _original.TempoExecucao = sw.Elapsed.ToString(@"hh\:mm\:ss\,ffff");
                    _original.TestesComSucesso = _javascriptHelper.TestesComSucesso;

                    _fitnessMin = _original.Fitness;

                    MelhorIndividuo = _original;
                    
                    break;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                    //Console.WriteLine("Erro na criação do original");

                    //Dorme um minuto e tenta de novo
                    Thread.Sleep(60000);
                    Console.WriteLine(" Falhou ao criar individuo. Tentando novamente.");

                    _javascriptHelper.Dispose();
                    _javascriptHelper = new JavascriptHelper(_diretorioFontes, _usarSetTimeout, false);
                    _javascriptHelper.ConfigurarGeracao();

                }

                contador++;
            }

        }

        /// <summary>
        /// Executa uma mutação no individuo
        /// </summary>
        /// <param name="sujeito"> </param>
        [HandleProcessCorruptedStateExceptions]
        private void ExecutarMutacao(Individuo sujeito)
        {

            try
            {
                int totalMutacoes = 1;
                string novaAst = "";


                while (novaAst == "" & totalMutacoes < 5)
                {
                    if (totalMutacoes > 1)
                        Console.WriteLine("          Tentativa {0} de executar mutação", totalMutacoes);

                    int no = Rand.Next(0, _total);

                    var avaliar = new Thread(() => novaAst = _javascriptHelper.ExecutarMutacaoExclusao(sujeito.Ast, no));
                    avaliar.Start();
                    avaliar.Join(_timeout * 1000); //timeout
                    
                    totalMutacoes++;
                }

                sujeito.Ast = novaAst;
                sujeito.CriadoPor = Operador.Mutacao;
            }
            catch (Exception ex)
            {
                Console.WriteLine("          Erro na Mutação");
                //Console.WriteLine("          {0}", ex);

                sujeito.Ast = "";
                sujeito.CriadoPor = Operador.Mutacao;
            }

            
        }

        /// <summary>
        /// Executa uma mutação no individuo
        /// </summary>
        /// <param name="sujeito"> </param>
        /// <param name="no"></param>
        private void ExecutarMutacao(Individuo sujeito, int no)
        {
            string novaAst = "";

            var executarMutacao = new Thread(() => novaAst = _javascriptHelper.ExecutarMutacaoExclusao(sujeito.Ast, no));
            executarMutacao.Start();
            executarMutacao.Join(_timeout * 1000); //timeout
            
            sujeito.Ast = novaAst;
            sujeito.CriadoPor = Operador.Mutacao;
        }

        /// <summary>
        /// Executa uma mutação no individuo
        /// </summary>
        /// <param name="ast"> </param>
        /// <param name="no"></param>
        public string ExecutarMutacaoNaFuncao(string ast, int no)
        {
            string novaAst = "";

            var executarMutacao = new Thread(() => novaAst = _javascriptHelper.ExecutarMutacaoExclusao(ast, no));
            executarMutacao.Start();
            executarMutacao.Join(_timeout * 1000); //timeout

            return novaAst;
        }


        /// <summary>
        /// Executa o Cruzamento
        /// </summary>
        /// <param name="pai"></param>
        /// <param name="mae"></param>
        /// <param name="filhoPai"></param>
        /// <param name="filhoMae"></param>
        [HandleProcessCorruptedStateExceptions]
        private void ExecutarCruzamento(Individuo pai, Individuo mae, out Individuo filhoPai, out Individuo filhoMae)
        {
            string c1 = "", c2 = "";
            
            filhoPai = pai.Clone();
            filhoPai.CriadoPor = Operador.Cruzamento;

            filhoMae = mae.Clone();
            filhoMae.CriadoPor = Operador.Cruzamento;

            try
            {
               
                var totalPai = Rand.Next(0, _total);
                var totalMae = Rand.Next(0, _total);
                _javascriptHelper.ExecutarCrossOver(pai.Ast, mae.Ast, totalPai, totalMae, out c1, out c2);
            }
            catch (Exception ex)
            {
                Console.WriteLine("          Erro ao executar cruzamento");
                //Console.Error(ex.ToString());
            }

            filhoPai.Ast = c1;
            filhoMae.Ast = c2;
            
        }

        /// <summary>
        /// Executa os testes com o sujeito e preenche sua propriedade Fitness Retorna o valor dessa propriedade
        /// </summary>
        /// <param name="indice"></param>
        /// <param name="sujeito"></param>
        [HandleProcessCorruptedStateExceptions]
        private double AvaliarIndividuo(int indice, Individuo sujeito)
        {
            const int total = 5;
            var fits = new double[total];

            fits[0] = ExecutarTestesParaIndividuoEspecifico(indice, sujeito);

            //Falhou em testes
            if (fits[0].Equals(120000))
            {
                sujeito.Fitness = 120000;
                return 120000;
            }

            //Igual ao original
            if (fits[0].Equals(_fitnessMin))
            {
                sujeito.Fitness = _fitnessMin;
                return _fitnessMin;
            }

            //Realmente executar
            for (int i = 0; i < total; i++)
            {
                fits[i] = ExecutarTestesParaIndividuoEspecifico(indice, sujeito);
                Console.WriteLine("             {0}-{1}", i, fits[i]);
            }

            sujeito.Fitness = fits.Average();
            Console.WriteLine(string.Format("            FIT:{0}     | CTs: {1}      | T: {2}", sujeito.Fitness, sujeito.TestesComSucesso, sujeito.TempoExecucao));

            return sujeito.Fitness;
        }

        /// <summary>
        /// Executa os testes
        /// </summary>
        /// <param name="indice"></param>
        /// <param name="sujeito"></param>
        /// <returns></returns>
        private double ExecutarTestesParaIndividuoEspecifico(int indice, Individuo sujeito)
        {
            var sw = new Stopwatch();
            

            const long valorFitFalha = 120000;

            var caminhoNovoAvaliado = GerarCodigo(sujeito);

            #region Codigo Vazio [sujeito inválido]

            if (string.IsNullOrEmpty(sujeito.Codigo) | (!File.Exists(caminhoNovoAvaliado)))
            {
                Console.WriteLine("              Codigo Vazio");

                sujeito.Fitness = valorFitFalha;
                sujeito.TestesComSucesso = 0;
                sujeito.TempoExecucao = sw.Elapsed.ToString(@"hh\:mm\:ss\,ffff");

                Console.WriteLine(string.Format("            FIT:{0}       | CTs: {1}            | T: {2}", sujeito.Fitness,
                                           sujeito.TestesComSucesso, sujeito.TempoExecucao));

                CriarLinhaExcel(indice, sujeito, sujeito.TestesComSucesso, sujeito.TempoExecucao);

                return sujeito.Fitness;
            }

            #endregion

            #region Igual ao Original

            if (indice > 0 & _original.Codigo.Equals(sujeito.Codigo))
            {
                Console.WriteLine("              Igual ao Original");

                sujeito.TempoExecucao = _original.TempoExecucao;
                sujeito.TestesComSucesso = _original.TestesComSucesso;
                sujeito.Fitness = _original.Fitness;
                Console.WriteLine(string.Format("            FIT:{0}       | CTs: {1}            | T: {2}", sujeito.Fitness,
                                           sujeito.TestesComSucesso, sujeito.TempoExecucao));

                CriarLinhaExcel(indice, sujeito, sujeito.TestesComSucesso, sujeito.TempoExecucao);

                return sujeito.Fitness;
            }

            #endregion

            #region realmente executar os testes então

            try
            {
                _javascriptHelper.ConfigurarTimeOut(_timeout);
                _javascriptHelper.ConfigurarMelhorFit(_fitnessMin);

                //Console.WriteLine("              Avaliando via testes");
                sw.Start();
                var avaliar =
                    new Thread(() => sujeito.Fitness = _javascriptHelper.ExecutarTestes(caminhoNovoAvaliado, _caminhoScriptTestes));
                avaliar.Start();
                avaliar.Join(_timeout*1000); //timeout

                sw.Stop();

                //Console.WriteLine("              Executou até o final: {0}", jHelper.ExecutouTestesAteFinal);

                if (!_javascriptHelper.ExecutouTestesAteFinal)
                    sujeito.Fitness = valorFitFalha;

                if (_javascriptHelper.ExecutouTestesAteFinal && _javascriptHelper.TestesComFalha > 0)
                    sujeito.Fitness = valorFitFalha; //+ jHelper.TestesComFalha;

                sujeito.TestesComSucesso = _javascriptHelper.TestesComSucesso;
                sujeito.TempoExecucao = sw.Elapsed.ToString(@"hh\:mm\:ss\,ffff");
            }
            catch (Exception ex)
            {
                //Console.WriteLine("              Executou até o final: {0}", jHelper.ExecutouTestesAteFinal);

                sujeito.Fitness = valorFitFalha;
                sujeito.TestesComSucesso = _javascriptHelper != null ? _javascriptHelper.TestesComSucesso : 0;
                sujeito.TempoExecucao = sw.Elapsed.ToString(@"hh\:mm\:ss\,ffff");

                Console.WriteLine(ex);

            }

            #endregion

            CriarLinhaExcel(indice, sujeito, sujeito.TestesComSucesso, sujeito.TempoExecucao);

            return sujeito.Fitness;
        }

        /// <summary>
        /// Gera o arquivo de código para avaliação
        /// </summary>
        /// <param name="sujeito"></param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public string GerarCodigo(Individuo sujeito)
        {
            var caminhoNovoAvaliado = string.Format("{0}\\{1}.js", _diretorioExecucao, sujeito.Id);

            try
            {
                sujeito.Codigo = _javascriptHelper.GerarCodigo(sujeito.Ast);

                if (!string.IsNullOrEmpty(sujeito.Codigo))
                {
                    File.WriteAllText(caminhoNovoAvaliado, sujeito.Codigo);
                    sujeito.Arquivo = caminhoNovoAvaliado;
                }
                else
                {
                    sujeito.Arquivo = "";
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
                Console.WriteLine("AST invalida. Codigo nao gerado");
                caminhoNovoAvaliado = "";
            }

            return caminhoNovoAvaliado;
        }

        /// <summary>
        /// Libera os objetos
        /// </summary>
        public void Dispose()
        {
            
        }

        /// <summary>
        /// Configura o otimizador para entender que está em um loop de otimização
        /// </summary>
        /// <param name="i"></param>
        public void ConfigurarRodada(int i)
        {
            RodadaGlobalExterna = i;

            _diretorioExecucao = i + "_" + _diretorioExecucao;
        }

        /// <summary>
        /// Configura o otimizador para uma determinada heuristica
        /// </summary>
        /// <param name="heuristica"></param>
        public void ConfigurarHeuristica(string heuristica)
        {
            Heuristica = heuristica;
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

            #region Inclui no  CSV

            var myExport = new CsvExport();

            myExport.AddRow();

            myExport["Geracao"] = _generationCount;
            myExport["Individuo"] = sujeito.Arquivo;
            myExport["Operacao"] = sujeito.CriadoPor.ToString();
            myExport["Fitness"] = sujeito.Fitness;
            myExport["Tempo"] = tempoTotal;
            myExport["Testes"] = testesComSucesso;

            myExport.ExportToFile(Path.Combine(_diretorioExecucao, "resultados.csv"));

            #endregion    
        }


    }
}
