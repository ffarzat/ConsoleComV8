using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ClearScript.Manager;
using ClearScript.Manager.Caching;
using ClearScript.Manager.Loaders;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using NLog;
using Newtonsoft.Json.Linq;
using Timer = System.Timers.Timer;

namespace Otimizacao.Javascript
{
    /// <summary>
    /// Rápido Helper para Js
    /// </summary>
    /// <remarks>
    /// [clearInterval, clearTimeout, setInterval, setTimeout]  seguiram as especificações da Mozilla em [https://developer.mozilla.org/en-US/docs/Web/API/WindowTimers/setTimeout#JavaScript_Content]
    /// </remarks>
    public class JavascriptHelper: IDisposable

    {
        /// <summary>
        /// Para testes
        /// </summary>
        public string CurrentThreadId {
            get
            {
                //Console.WriteLine("ENGINE_ThreadId : {0}", Thread.CurrentThread.ManagedThreadId);
               return Thread.CurrentThread.ManagedThreadId.ToString();
            }
        }

        /// <summary>
        /// V8 chrome engine
        /// </summary>
        private V8ScriptEngine _engine;

        /// <summary>
        /// String com o Json da árvore do Javascript processada
        /// </summary>
        public string JsonAst { get; set; }

        /// <summary>
        /// Árvore tipada do Esprima
        /// </summary>
        public dynamic Program { get; set; }

        /// <summary>
        /// Codigo regerado pelo Escodegen
        /// </summary>
        public string Codigo { get; set; }

        /// <summary>
        /// Total de testes unitários passados para otimização
        /// </summary>
        public int TotalTestes { get; set; }

        /// <summary>
        /// Testes que falharam na execução
        /// </summary>
        public int TestesComFalha { get; set; }

        /// <summary>
        /// Testes que passaram na execução
        /// </summary>
        public int TestesComSucesso { get; set; }

        /// <summary>
        /// Lista com os detalhes dos erros na execução
        /// </summary>
        public List<string> FalhasDosTestes { get; internal set; }

        /// <summary>
        /// Guarda os timers para simular o SetTimeout;
        /// </summary>
        private Dictionary<int, bool> _timers;

        /// <summary>
        /// Funções para futura execução via SetTimout
        /// </summary>
        private Dictionary<int, object> _timeOutCodes; 

        /// <summary>
        /// NLog Logger
        /// </summary>
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Cache dos arquivos em disco
        /// </summary>
        private Dictionary<string, string> _cacheCodigos;

        /// <summary>
        /// Manager da ScriptEngine
        /// </summary>
        //private RuntimeManager _manager;

        /// <summary>
        /// Diretório onde os scripts estão
        /// </summary>
        private string _diretorioExecucao;

        /// <summary>
        /// Tempo para timout nos testes
        /// </summary>
        private int _timeoutTestes;

        /// <summary>
        /// Valor de multa para Calculo da Fitness
        /// </summary>
        private Int64 _fitTopValue;

        /// <summary>
        /// Total de Nós de um
        /// </summary>
        public int TotalDeNos { get; set; } 

        /// <summary>
        /// Construtor, configura o Helper para posterior execuçao
        /// </summary>
        /// <param name="diretorioJavascripts">Diretório onde estão os arquivos em js</param>
        public JavascriptHelper(string diretorioJavascripts)
        {
            Carregar(diretorioJavascripts, false, false);
        }

        /// <summary>
        /// Construtor, configura o Helper para posterior execuçao
        /// </summary>
        /// <param name="diretorioJavascripts">Diretório onde estão os arquivos em js</param>
        /// <param name="setTimeout">Habilitar a função global setTimeout</param>
        /// <param name="setInterval">Habilitar a função global setInterval</param>
        public JavascriptHelper(string diretorioJavascripts, bool setTimeout, bool setInterval)
        {
            Carregar(diretorioJavascripts, setTimeout, setInterval);
        }

        /// <summary>
        /// configura o Helper para posterior execuçao
        /// </summary>
        /// <param name="diretorioJavascripts">Diretório onde estão os arquivos em js</param>
        /// <param name="setTimeout">Habilitar a função global setTimeout</param>
        /// <param name="setInterval">Habilitar a função global setInterval</param>
        private void Carregar(string diretorioJavascripts, bool setTimeout, bool setInterval)
        {
            _diretorioExecucao = diretorioJavascripts;
            _timeoutTestes = int.MaxValue;

            //O manager vai compilar e cachear as bibliotecas
            //_manager = new RuntimeManager(new ManualManagerSettings() { MaxExecutableBytes = (1000000000 * 2)});
            _engine = new V8ScriptEngine();
            RequireManager.ClearPackages(); //garantir uma execução limpa

            #region Ler arquivos Js
            
            _cacheCodigos = new Dictionary<string, string>();

            foreach (var enumerateFile in Directory.EnumerateFiles(diretorioJavascripts, "*.js"))
            {
                _cacheCodigos.Add(Path.GetFileNameWithoutExtension(enumerateFile), File.ReadAllText(enumerateFile));
                //_manager.Compile(Path.GetFileNameWithoutExtension(enumerateFile), File.ReadAllText(enumerateFile), true, int.MaxValue);
            }

            #endregion

            #region Configura a Engine com o JavascriptHelper e Console, Settimeout e etc
            


            FalhasDosTestes = new List<string>();
            _timers = new Dictionary<int, bool>();

            _engine.AddHostObject("javascriptHelper", this);
            
            _engine.Execute(@"'use strict';
                        function console() {
                          if (!(this instanceof console)) {
                            return new Console();
                          }
                        }

                        console.prototype.log = function(args, args1, args2) {
                          javascriptHelper.Escrever(args, args1, args2);
                        };");

            _engine.Execute("var console = new console();");
            _engine.Execute("var GLOBAL = this;");

            _engine.Execute(@"var stFunctionsCallBack = new Array();");

            if (setTimeout)
            {
                _engine.Execute(@"var setTimeout = function (funcToCall, millis) {
                                                var textoId = javascriptHelper.CurrentThreadId;
                                                var idlocal = javascriptHelper.SetTimeout(millis);
                                                stFunctionsCallBack.push(funcToCall);
                                                return idlocal;
                            };");

                _engine.Execute(@"var clearTimeout = function(id) { javascriptHelper.ClearTimeout(id);};");
            }

            if (setInterval)
            {
                _engine.Execute(@"var setInterval = function (funcToCall, millis) {
                                                
                                                var idlocal = javascriptHelper.SetTimeout(millis);
                                                var funcaoTimeout = function() { funcToCall(); setTimeout(funcToCall, millis); };
                                                stFunctionsCallBack.push(funcaoTimeout);
                                                return idlocal;
                            };");

                _engine.Execute(@"var clearInterval = function(id) { javascriptHelper.ClearTimeout(id);};");
            }
            #endregion
        }

        /// <summary>
        /// Registra um pacote em js para emular o funcionamento do RequireJs
        /// </summary>
        /// <param name="id"></param>
        /// <param name="arquivoJs"></param>
        private void RegistarScript(string id, string arquivoJs)
        {
            RequireManager.RegisterPackage(new RequiredPackage { PackageId = id, ScriptUri = string.Format("{0}\\{1}", _diretorioExecucao, arquivoJs) });
        }

        /// <summary>
        /// Metodo para futura geraco de código e mutantes
        /// </summary>
        public void ConfigurarGeracao()
        {
            #region Registra os pacotes

            var esprima = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, _diretorioExecucao,"esprima.js"));
            var escodegen = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, _diretorioExecucao,"escodegen.js"));
            var estraverse = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, _diretorioExecucao,"estraverse.js"));
            var ast = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, _diretorioExecucao,"ast.js"));
            var code = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, _diretorioExecucao,"code.js"));
            var keyword = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, _diretorioExecucao,"keyword.js"));

            #endregion

            #region Congigura o Escodegen e o Esprima

            //var engine = _manager.GetEngine();
            
            _engine.Execute(@"   var ObjEstraverse = {};
                                var ObjEscodegen = {};
                                var ObjCode = {};
                                var ObjAst = {};
                                var ObjKeyword = {};
                            ");

            _engine.Execute(code);
            _engine.Execute(ast);
            _engine.Execute(keyword);
            _engine.Execute(estraverse);

            _engine.Execute(@"
                            var Objutils = {};
                            Objutils.ast = ObjAst;
                            Objutils.code = ObjCode;
                            Objutils.keyword = ObjKeyword;
                            
            ");

            _engine.Execute(escodegen);
            _engine.Execute(esprima);

            _engine.Execute(@"        option = {
                                                comment: true,
                                                format: {
                                                    indent: {
                                                        style: '    '
                                                    },
                                                    quotes: 'auto'
                                                }
                                            };");

            #endregion
        }

        /// <summary>
        /// Gera a AST em Javascript do individuo
        /// </summary>
        /// <param name="codigoIndividuo"></param>
        /// <returns></returns>
        public string GerarAst(string codigoIndividuo)
        {
            var sw = new Stopwatch();
            sw.Start();

            //var engine = _manager.GetEngine();
            var esprimaParse = string.Format(@"var syntax = esprima.parse({0}, {{ raw: true, tokens: true, range: true, loc:true, comment: true }});", EncodeJsString(codigoIndividuo));
            _engine.Execute(esprimaParse);
            _engine.Execute("javascriptHelper.JsonAst = JSON.stringify(syntax);");

            sw.Stop();

            //Log(string.Format("ast gerada com sucesso"));
            //Log(string.Format(" {0} ms", sw.Elapsed.TotalMilliseconds));

            return JsonAst;
        }

        /// <summary>
        /// Identa uma string no formato Json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public string FormatarStringJson(string json)
        {
            return JToken.Parse(json).ToString();
        }

        /// <summary>
        /// Executa a exclusao de um nó específico na árvore
        /// </summary>
        /// <param name="ast">árvore no formato do esprima</param>
        /// <param name="randonNode">Linha para excluir</param>
        /// <returns></returns>
        public string ExecutarMutacaoExclusao(string ast, int randonNode)
        {
            try
            {
                _engine.Execute(@"

                    var ast = JSON.parse(#ast);

                    var indent = 0;
                    var counter = 0;
                    ObjEstraverse.replace(ast, {
                        enter: function(node, parent) {
                            
                            //javascriptHelper.Escrever('{0}', JSON.stringify(node));
                            //node.type == 'VariableDeclaration' && 

                            if(counter > #randonNode)
                            {
                                //javascriptHelper.Escrever('             Excluindo nó: {0}', JSON.stringify(node));
                                node.type = 'EmptyStatement';
                                this.break();
                                return node;

                            }

                            counter++;
                            indent += 4;
                        },
                        leave: function(node, parent) {
                            indent -= 4;
                        }
                    });

                    javascriptHelper.JsonAst = JSON.stringify(ast);
                    ".Replace("#ast", this.EncodeJsString(ast)).Replace("#randonNode", randonNode.ToString()));
            }
            catch (Exception ex)
            {
                _logger.Trace(ex.ToString());
                return "";
            }
            

           return JsonAst;
        }

        /// <summary>
        /// Conta o total de Nós de uma Ast
        /// </summary>
        /// <param name="ast">árvore no formato do esprima</param>
        /// <returns></returns>
        public int ContarNos(string ast)
        {
            try
            {
                _engine.Execute(@"

                    var ast = JSON.parse(#ast);

                    var indent = 0;
                    var counter = 0;

                    ObjEstraverse.replace(ast, {
                        enter: function(node, parent) {
                            counter++;
                        }
                    });

                    javascriptHelper.TotalDeNos = counter;
                    ".Replace("#ast", this.EncodeJsString(ast)));
            }
            catch (Exception ex )
            {
                
                _logger.Trace(ex.ToString());
                return 0;
            }
            

            return TotalDeNos;
        }

        /// <summary>
        /// Exeucta um crossOver para gerar dois novos individuos trocando material entre o pai e mae
        /// </summary>
        /// <param name="astPai">Ast que representa o Pai</param>
        /// <param name="astMae">Ast que representa a mae</param>
        /// <param name="randonNodePai">Nó aleatório escolhido no pai</param>
        /// <param name="randonNodeMae">Nó aleatório escolhido na mãe</param>
        /// <param name="astPrimeiroFilho">Pai com o nó da mãe</param>
        /// <param name="astSegundoFilho">Mãe com o nó do pai</param>
        /// <returns></returns>
        public void ExecutarCrossOver(string astPai, string astMae, int randonNodePai, int randonNodeMae, out string astPrimeiroFilho, out string astSegundoFilho)
        {
            //var engine = _manager.GetEngine();

            astPrimeiroFilho = "";
            astSegundoFilho = "";

            try
            {
                #region Recupero o nó no pai

                _engine.Execute("var nodePai = {type:'EmptyStatement'};");

                _engine.Execute("var astPai = JSON.parse(#astPai);"
                    .Replace("#astPai", this.EncodeJsString(astPai))
                    );

                _engine.Execute(@"

                    var counter = 0;
                    ObjEstraverse.replace(astPai, {
                        enter: function(node, parent) {

                            if(counter > #randonNodePai)
                            {
                                nodePai = node;
                                this.break();
                            }

                            counter++;
                        }
                    });
                    "
                    .Replace("#randonNodePai", randonNodePai.ToString())

                         );

                #endregion

                #region Recupero o nó na mãe e troco pelo do Pai
                _engine.Execute("var nodeMae = {type : 'EmptyStatement'};");
                _engine.Execute("var astMae = JSON.parse(#astMae);"
                     .Replace("#astMae", this.EncodeJsString(astMae))
                     );

                _engine.Execute(@"

                    var counter = 0;
                    ObjEstraverse.replace(astMae, {
                        enter: function(node, parent) {

                            if(counter > #randonNodeMae)
                            {
                                nodeMae = node;
                                this.break();
                                return nodePai;
                            }

                            counter++;
                        }
                    });
                        javascriptHelper.JsonAst = JSON.stringify(astMae);
                    "

                    .Replace("#randonNodeMae", randonNodeMae.ToString())

                    );

                //Troco na mae
                astSegundoFilho = JsonAst;

                #endregion

                #region Troco agora no pai

                _engine.Execute(@"

                    var counter = 0;
                    ObjEstraverse.replace(astPai, {
                        enter: function(node, parent) {

                            if(counter > #randonNodePai)
                            {
                                this.break();
                                return nodeMae;
                            }

                            counter++;
                        }
                    });

                    javascriptHelper.JsonAst = JSON.stringify(astPai);
                    "
                        .Replace("#astPai", this.EncodeJsString(astPai))
                        .Replace("#randonNodePai", randonNodePai.ToString())

                        );

                astPrimeiroFilho = JsonAst;
                #endregion
            }
            catch (ScriptEngineException ex)
            {
                _logger.Trace(ex);
            }
            catch (Exception ex)
            {
                _logger.Trace(ex);
            }

        }

        /// <summary>
        /// Baseado na AST gera o código do individuo
        /// </summary>
        /// <param name="astJson">AST no formato do Esprima</param>
        /// <returns></returns>
        public string GerarCodigo(string astJson)
        {

            var sw = new Stopwatch();
            sw.Start();

            this.JsonAst = astJson;

            try
            {
                _engine.Execute("var syntax = JSON.parse(javascriptHelper.JsonAst);");
                _engine.Execute("syntax = ObjEscodegen.attachComments(syntax, syntax.comments, syntax.tokens);");
                _engine.Execute("var code = ObjEscodegen.generate(syntax, option);");
                _engine.Execute("javascriptHelper.Codigo = code;");
            }
            catch (Exception ex)
            {
                _logger.Trace(ex);
                return "";
            }


            sw.Stop();

            //Log(string.Format("Codigo gerado com sucesso"));
            //Log(string.Format(" {0} ms", sw.Elapsed.TotalMilliseconds));

            return Codigo;
        }

        /// <summary>
        /// Executa os testes unitário para o individuo
        /// </summary>
        /// <param name="nomeArquivoIndividuo">Nome do arquivo.js que representa o individuo</param>
        /// <param name="nomeDoArquivoTestes">Nome do arquivo.js que representa a lista de testes para executar</param>
        /// <returns></returns>
        public Int64 ExecutarTestes(string nomeArquivoIndividuo, string nomeDoArquivoTestes)
        {
           
            var sw = new Stopwatch();
            sw.Start();

            #region Configura o QUnit



            _engine.Execute(_cacheCodigos["Qunit"]);
            _engine.Execute(_cacheCodigos["qunit-extras"]);

            //_manager.ExecuteCompiled("Qunit");
            //_manager.ExecuteCompiled("qunit-extras");
            
            _engine.Execute(@"   
                                    var total, sucesso, falha;

                                    QUnit.done(function( details ) {
                                    //console.log('=============================================');
                                    //console.log('Total:' + details.total);
                                    //console.log('Falha:' + details.failed);
                                    //console.log('Sucesso:' + details.passed);
                                    //console.log('Tempo:' + details.runtime);
                                       
                                    javascriptHelper.TotalTestes = details.total;
                                    javascriptHelper.TestesComSucesso = details.passed;
                                    javascriptHelper.TestesComFalha = details.failed;

                                });

                                QUnit.log(function( details ) {
                                  if ( details.result ) {
                                    return;
                                  }
                                  var loc = details.module + ': ' + details.name + ': ',
                                    output = 'FAILED: ' + loc + ( details.message ? details.message + ', ' : '' );
 
                                  if ( details.actual ) {
                                    output += 'expected: ' + details.expected + ', actual: ' + details.actual;
                                  }
                                  if ( details.source ) {
                                    output += ', ' + details.source;
                                  }

                                    //console.log('=============================================');
                                    //console.log( output );

                                    javascriptHelper.AdicionarDetalheDeFalha(output);
                                });



                                QUnit.config.autostart = false;
                                QUnit.config.ignoreGlobalErrors = true;
                        ");
            #endregion

            #region Carrega Individuo e executa os Testes
            
            try
            {

                //_manager.Compile(Path.GetFileNameWithoutExtension(nomeArquivoIndividuo), File.ReadAllText(nomeArquivoIndividuo), true, 60);
                //_manager.ExecuteCompiled(Path.GetFileNameWithoutExtension(nomeArquivoIndividuo));
                _engine.Execute(File.ReadAllText(nomeArquivoIndividuo));

                //_manager.ExecuteCompiled(Path.GetFileNameWithoutExtension(nomeDoArquivoTestes));
                _engine.Execute(_cacheCodigos[Path.GetFileNameWithoutExtension(nomeDoArquivoTestes)]);

                //Escrever("Iniciando os testes");
                //Escrever("_timers.Count {0}", _timers.Count);

                _engine.Execute(@"   QUnit.load();
                                QUnit.start();
                ");
            }
            catch (ScriptEngineException ex)
            {
                _logger.Trace(ex.ErrorDetails);
                _logger.Trace(ex.ToString());
                return _fitTopValue + 1000;
            }
            catch (Exception ex)
            {
                _logger.Trace(ex.ToString());
                return _fitTopValue = 1000;
            }

            while (GetTimersCount() > 0 & sw.Elapsed.Seconds <= _timeoutTestes)
            {
                Thread.Sleep(5);
            }
            
            //Escrever("Encerrando os testes");

            #endregion
            
            sw.Stop();

            _logger.Trace(string.Format("           Total:{0}, Sucesso: {1}, Falha: {2}", this.TotalTestes, this.TestesComSucesso, this.TestesComFalha));
            _logger.Trace(string.Format("           {0} segundos para avaliar o individuo {1}", sw.Elapsed.Seconds, nomeArquivoIndividuo));

            //this.FalhasDosTestes.ForEach(this.Log);

            if (TestesComFalha > 0 | TestesComSucesso == 0 | TestesComSucesso != TotalTestes)
                return _fitTopValue + TestesComFalha;

            return (sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Retorno o total de Timers ainda para executar
        /// </summary>
        /// <returns></returns>
        private int GetTimersCount()
        {
            int retorno = 0;
            
            lock (_timers)
            {
                retorno = _timers.Count(pair => pair.Value);
            }
            
            return retorno;
        }

        /// <summary>
        /// Cancela a execução de um timeout
        /// </summary>
        /// <param name="id"></param>
        public void ClearTimeout(string id)
        {
            int idLocal = int.Parse(id);

            lock (_timers)
            {
                _timers[idLocal] = false;
            }
        }

        /// <summary>
        /// Simula o timeout
        /// </summary>
        /// <param name="miliseconds">tempo em ms</param>
        public string SetTimeout(int miliseconds)
        {
            int id = _timers.Count;
            //Escrever("  Settimeout: id:{0}, ({1}) ms", id, miliseconds);

            lock (_timers)
            {
                _timers.Add(id, true);
                //Escrever("_timers.Count {0}", _timers.Count);
            }

            //Escrever("  ManagedThreadId : [{0}]", Thread.CurrentThread.ManagedThreadId.ToString());

            var th = new Thread(() =>
                {
                    Thread.Sleep(miliseconds);
                    JavascriptHelper_Elapsed(id);
                });

            th.Priority = ThreadPriority.Highest;
            th.IsBackground = false;
            th.Start();
            //th.Join(50);

            return id.ToString();
        }

        /// <summary>
        /// Quando o timer dispara
        /// </summary>
        void JavascriptHelper_Elapsed(int id)
        {
            //Escrever("      Executar timer: id:{0}:", id);
            //Escrever("      ManagedThreadId : [{0}]", Thread.CurrentThread.ManagedThreadId.ToString());

            lock (_timers)
            {
                if (_timers[id])
                {
                    //Escrever("      Executando timer: id:{0}, ({1})", id, DateTime.Now.ToString("HH:mm:ss.ffff"));
                    //_engine.Execute(string.Format("javascriptHelper.Escrever('          Deveria ter disparado: ' + stFunctionsCallBack[{0}]);", id));
                    try
                    {
                        _engine.Execute(string.Format("stFunctionsCallBack[{0}]();", id));
                    }
                    catch (Exception ex)
                    {
                        _logger.Trace("         Erro na execução do SetTimeout id {0}", id);
                        _logger.Trace(ex);
                    }

                    
                    //Escrever("      Encerrado timer: id:{0}, ({1})", id, DateTime.Now.ToString("HH:mm:ss.ffff"));
                    _timers[id] = false;
                }
                
                
            }
        }

        /// <summary>
        /// Simula o comportamento do Console.log do javascript
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="args1"></param>
        /// <param name="args2"></param>
        public void Escrever(string arg, object args1 = null, object args2 = null)
        {
            var consoleOut = arg.Replace("%s", "{0}");
            string one = args1 == null ? "" : args1.ToString();
            string two = args2 == null ? "" : args2.ToString();
            _logger.Info(consoleOut, one, two);
        }

        /// <summary>
        /// Grava um Log do tipo Info direto no arquivo
        /// </summary>
        public void Log(string mensagem)
        {
            _logger.Info(mensagem);
        }

        /// <summary>
        /// Encodes a string to be represented as a string literal. The format
        /// is essentially a JSON string.
        /// 
        /// The string returned includes outer quotes 
        /// Example Output: "Hello \"Rick\"!\r\nRock on"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string EncodeJsString(string s)
        {
            var sb = new StringBuilder();
            sb.Append("\"");
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");

            return sb.ToString();
        }

        /// <summary>
        /// Adiciona uma falha a lista
        /// </summary>
        /// <param name="detalhes"></param>
        public void AdicionarDetalheDeFalha(string detalhes)
        {
            this.FalhasDosTestes.Add(detalhes);
        }

        /// <summary>
        /// Dipor do objeto
        /// </summary>
        public void Dispose()
        {
            //_manager.Dispose();
            _engine.Dispose();

            //GC.Collect(GC.MaxGeneration);
            //GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Timeout para execução dos testes em segundos
        /// </summary>
        /// <param name="i"></param>
        public void ConfigurarTimeOut(int i)
        {
            _timeoutTestes = i;
        }

        /// <summary>
        /// Configura o limite superior da Fit
        /// </summary>
        /// <param name="fitnessMin"></param>
        public void ConfigurarMelhorFit(Int64 fitnessMin)
        {
            _fitTopValue = fitnessMin;
        }
    }
}
