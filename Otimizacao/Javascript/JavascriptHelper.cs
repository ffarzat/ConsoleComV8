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
    public class JavascriptHelper
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
        /// Manager da ScriptEngine
        /// </summary>
        private RuntimeManager _manager;

        /// <summary>
        /// Diretório onde os scripts estão
        /// </summary>
        private string _diretorioExecucao;

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

            //O manager vai compilar e cachear as bibliotecas
            _manager = new RuntimeManager(new ManualManagerSettings() { ScriptCacheMaxCount = 100, ScriptCacheExpirationSeconds = Int16.MaxValue });
            _engine = _manager.GetEngine();
            RequireManager.ClearPackages(); //garantir uma execução limpa

            #region Ler arquivos Js
            //_javascripts = new Dictionary<string, string>();

            foreach (var enumerateFile in Directory.EnumerateFiles(diretorioJavascripts, "*.js"))
            {
                _manager.Compile(Path.GetFileNameWithoutExtension(enumerateFile), File.ReadAllText(enumerateFile), true, int.MaxValue);
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

            //RegistarScript("esprima", "esprima.js");
            //RegistarScript("estraverse", "estraverse.js");
            //RegistarScript("esutils", "utils.js");
            //RegistarScript("ast", "ast.js");
            //RegistarScript("code", "code.js");
            //RegistarScript("keyword", "keyword.js");

            
            //var scriptTestCode = File.ReadAllText("core-test.js");
            //var qunit = File.ReadAllText("qunit-1.18.0.js");
            //var console = File.ReadAllText("Console.js");
            var esprima = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, _diretorioExecucao,"esprima.js"));
            var escodegen = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, _diretorioExecucao,"escodegen.js"));
            var estraverse = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, _diretorioExecucao,"estraverse.js"));
            var ast = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, _diretorioExecucao,"ast.js"));
            var code = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, _diretorioExecucao,"code.js"));
            var keyword = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, _diretorioExecucao,"keyword.js"));

            #endregion

            #region Congigura o Escodegen e o Esprima

            var engine = _manager.GetEngine();
            
            engine.Execute(@"   var ObjEstraverse = {};
                                var ObjEscodegen = {};
                                var ObjCode = {};
                                var ObjAst = {};
                                var ObjKeyword = {};
                            ");

            engine.Execute(code);
            engine.Execute(ast);
            engine.Execute(keyword);
            engine.Execute(estraverse);

            engine.Execute(@"
                            var Objutils = {};
                            Objutils.ast = ObjAst;
                            Objutils.code = ObjCode;
                            Objutils.keyword = ObjKeyword;
                            
            ");

            engine.Execute(escodegen);
            engine.Execute(esprima);

            engine.Execute(@"        option = {
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

            var engine = _manager.GetEngine();
            var esprimaParse = string.Format(@"var syntax = esprima.parse({0}, {{ raw: true, tokens: true, range: true, loc:true, comment: true }});", EncodeJsString(codigoIndividuo));
            engine.Execute(esprimaParse);
            engine.Execute("javascriptHelper.JsonAst = JSON.stringify(syntax);");

            sw.Stop();

            Log(string.Format("ast gerada com sucesso"));
            Log(string.Format(" {0} ms", sw.Elapsed.TotalMilliseconds));

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
        /// <returns></returns>
        public async Task<string> ExecutarMutacaoExclusao(string ast)
        {
            
            RegistarScript("asttypes", "main.js");
            RegistarScript("assert", "assert.js");
            RegistarScript("util", "util.js");
            RegistarScript("isBuffer", "isBuffer.js");
            RegistarScript("inherits", "inherits.js");

            RegistarScript("core", "def/core.js");
            RegistarScript("es6", "def/es6.js");
            RegistarScript("es7", "def/es7.js");
            RegistarScript("mozilla", "def/mozilla.js");
            RegistarScript("e4x", "def/e4x.js");
            RegistarScript("fbharmony", "def/fb-harmony.js");
            RegistarScript("babel", "def/babel.js");
            RegistarScript("esprima", "def/esprima.js");

            RegistarScript("path", "lib/path.js");
            RegistarScript("scope", "lib/scope.js");
            RegistarScript("shared", "lib/shared.js");
            RegistarScript("types", "lib/types.js");
            RegistarScript("equiv", "lib/equiv.js");
            RegistarScript("nodepath", "lib/node-path.js");
            RegistarScript("pathvisitor", "lib/path-visitor.js");


            await _manager.ExecuteAsync("", @"  var ast = JSON.parse(javascriptHelper.JsonAst);
                                                require('asttypes');

                                                var n = types.namedTypes;

                                                types.visit(ast, {
                                                    // This method will be called for any node with .type 'MemberExpression':
                                                    visitFunction: function(path) {


                                                        var node = path.node;

                                                        javascriptHelper.Escrever('{0}', JSON.stringify(path.name));

                                                        path.prune();

                                                        
                                                        this.traverse(path);

                                                        
                                                    }
                                                });

                                                javascriptHelper.JsonAst = JSON.stringify(ast);

            ");

           return JsonAst;
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


            var engine = _manager.GetEngine();
            this.JsonAst = astJson;

            engine.Execute("var syntax = JSON.parse(javascriptHelper.JsonAst);");
            engine.Execute("syntax = ObjEscodegen.attachComments(syntax, syntax.comments, syntax.tokens);");
            engine.Execute("var code = ObjEscodegen.generate(syntax, option);");
            engine.Execute("javascriptHelper.Codigo = code;");

            sw.Stop();

            Log(string.Format("Codigo gerado com sucesso"));
            Log(string.Format(" {0} ms", sw.Elapsed.TotalMilliseconds));

            return Codigo;
        }

        /// <summary>
        /// Executa os testes unitário para o individuo
        /// </summary>
        /// <param name="nomeArquivoIndividuo">Nome do arquivo.js que representa o individuo</param>
        /// <param name="nomeDoArquivoTestes">Nome do arquivo.js que representa a lista de testes para executar</param>
        /// <returns></returns>
        public bool ExecutarTestes(string nomeArquivoIndividuo, string nomeDoArquivoTestes)
        {
           
            var sw = new Stopwatch();
            sw.Start();

            #region Configura o QUnit

            _manager.ExecuteCompiled("Qunit");
            _manager.ExecuteCompiled("qunit-extras");
            
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

            #region Carrega o individuo
            _manager.ExecuteCompiled(Path.GetFileNameWithoutExtension(nomeArquivoIndividuo));
            #endregion

            #region Carrega e executa os Testes
            _manager.ExecuteCompiled(Path.GetFileNameWithoutExtension(nomeDoArquivoTestes));

            Escrever("Iniciando os testes");
            //Escrever("_timers.Count {0}", _timers.Count);

            _engine.Execute(@"   QUnit.load();
                                QUnit.start();
                ");

            //Escrever("_timers.Count {0}", _timers.Count);
            
            
            while (GetTimersCount() > 0)
            {
                Thread.Sleep(5);
            }
    
            

            
            Escrever("Encerrando os testes");

            #endregion
            
            sw.Stop();

            Log(string.Format("Total:{0}, Sucesso: {1}, Falha: {2}", this.TotalTestes, this.TestesComSucesso, this.TestesComFalha));
            Log(string.Format(" {0} segundos para avaliar o individuo {1}", sw.Elapsed.Seconds, nomeArquivoIndividuo));

            this.FalhasDosTestes.ForEach(this.Log);

            

            return true;
        }

        /// <summary>
        /// Executa um script por ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ExecutarScriptPorId(string id)
        {
            bool sucesso = false;
            try
            {
                sucesso = _manager.ExecuteCompiled(id);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw;
            }

            _logger.Info(string.Format("{0} executado com sucesso? {1}", id, sucesso));
            
            return sucesso;
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

                    _engine.Execute(string.Format("stFunctionsCallBack[{0}]();", id));

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
    }
}
