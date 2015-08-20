using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Microsoft.ClearScript.V8;
using NLog;
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
                Console.WriteLine("ENGINE_ThreadId : {0}", Thread.CurrentThread.ManagedThreadId);
               return Thread.CurrentThread.ManagedThreadId.ToString();
            }
        }

        /// <summary>
        /// Para fazer o lock do SetTimeout
        /// </summary>
        private object _timerLock = new object();

        /// <summary>
        /// Armazena os Javascripts carregados
        /// </summary>
        private Dictionary<string, string> _javascripts;

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
        private Dictionary<int, Timer> _timers;

        /// <summary>
        /// Funções para futura execução via SetTimout
        /// </summary>
        private Dictionary<int, object> _timeOutCodes; 

        /// <summary>
        /// NLog Logger
        /// </summary>
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Construtor, configura o Helper para posterior execuçao
        /// </summary>
        public JavascriptHelper(string diretorioJavascripts)
        {
            #region Ler arquivos Js
            _javascripts = new Dictionary<string, string>();

            foreach (var enumerateFile in Directory.EnumerateFiles(diretorioJavascripts, "*.js"))
            {
                _javascripts.Add(Path.GetFileName(enumerateFile) , File.ReadAllText(enumerateFile));    
            }

            #endregion

            #region Cria a Engine e configura com o JavascriptHelper e Console
            
            _engine = new V8ScriptEngine();
            FalhasDosTestes = new List<string>();
            _timers = new Dictionary<int, Timer>();

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

            _engine.Execute(@"var stFunctionsCallBack = new Array();");

            _engine.Execute(@"var setTimeout = function (funcToCall, millis) {
                                                var textoId = javascriptHelper.CurrentThreadId;
                                                var idlocal = javascriptHelper.SetTimeout(millis);
                                                stFunctionsCallBack.push(funcToCall);
                                                return idlocal;
                            };");

            #endregion
        }

        /// <summary>
        /// Metodo para futura geraco de código e mutantes
        /// </summary>
        public void configurarGeracao()
        {
            #region Congigura o Escodegen e o Esprima
//            Engine.Execute(@"   var ObjEstraverse = {};
//                                var ObjEscodegen = {};
//                                var ObjCode = {};
//                                var ObjAst = {};
//                                var ObjKeyword = {};
//
//                            ");

//            Engine.Execute(code);
//            Engine.Execute(ast);
//            Engine.Execute(keyword);
//            Engine.Execute(estraverse);

//            Engine.Execute(@"
//
//                            var Objutils = {};
//                            Objutils.ast = ObjAst;
//                            Objutils.code = ObjCode;
//                            Objutils.keyword = ObjKeyword;
//                            
//            ");


//            Engine.Execute(escodegen);
//            Engine.Execute(esprima);

//            Engine.Execute(@"        option = {
//                                                comment: true,
//                                                format: {
//                                                    indent: {
//                                                        style: '\t'
//                                                    },
//                                                    quotes: 'auto'
//                                                }
//                                            };");


//            var esprimaParse = string.Format(@"var syntax = esprima.parse({0}, {{ raw: true, tokens: true, range: true, comment: true }});", helper.EncodeJsString(scriptCode));
//            Engine.Execute(esprimaParse);
//            Engine.Execute("javascriptHelper.JsonAst = JSON.stringify(syntax);"); //Passo para o c#

            //helper.Program = JsonConvert.DeserializeObject<Otimizacao.EsprimaAST.Nodes.Program>(helper.JsonAst, new EsprimaAstConverter());
            //helper.Program = JsonConvert.DeserializeObject<dynamic>(helper.JsonAst);

            /*
            engine.Execute(astTraverse);
            
            engine.Execute(@"traverse(syntax, {pre: function(node, parent, prop, idx) {
                                console.log(node.type + (parent ? ' from parent ' + parent.type + ' via ' + prop + (idx !== undefined ? '[' + idx + ']' : '') : ''));
                        }});");
             */

            //Engine.Execute("syntax = ObjEscodegen.attachComments(syntax, syntax.comments, syntax.tokens);");
            //Engine.Execute("var code = ObjEscodegen.generate(syntax, option);");
            //Engine.Execute("javascriptHelper.Codigo = code;");

            #endregion
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

            _engine.Execute(_javascripts["Qunit.js"]);

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
            _engine.Execute(_javascripts[nomeArquivoIndividuo]);
            #endregion

            #region Carrega e executa os Testes
            _engine.Execute(_javascripts[nomeDoArquivoTestes]);

            _engine.Execute(@"   QUnit.load();
                                QUnit.start();
                ");
            #endregion
            
            sw.Stop();

            Log(string.Format("Total:{0}, Sucesso: {1}, Falha: {2}", this.TotalTestes, this.TestesComSucesso, this.TestesComFalha));
            Log(string.Format("{0} segundos para avaliar o individuo {1}", sw.Elapsed.Seconds, nomeArquivoIndividuo));

            this.FalhasDosTestes.ForEach(this.Log);

            bool gotLock = Monitor.TryEnter(_timerLock, TimeSpan.FromSeconds(60)); // consegue o lock? Pode sair

            return gotLock;
        }

        /// <summary>
        /// Simula o timeout
        /// </summary>
        /// <param name="miliseconds">tempo em ms</param>
        public string SetTimeout(int miliseconds)
        {
            Console.WriteLine("Helper_ThreadId : {0}", Thread.CurrentThread.ManagedThreadId);

                   
            int id = _timers.Count;
            var t = new Timer(miliseconds);
            
            t.Elapsed += JavascriptHelper_Elapsed;
            
            t.Start();

            _timers.Add(id, t);

           
            return id.ToString();
        }

        /// <summary>
        /// Quando o timer dispara
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void JavascriptHelper_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Monitor.TryEnter(_timerLock))
            {
                return;
            }

            try
            {
                Console.WriteLine("Timer_ThreadId : {0}", Thread.CurrentThread.ManagedThreadId);

                var item = _timers.Where(kp => kp.Value == (Timer)sender);

                if (item.Any())
                {
                    var id = item.First().Key;
                    var timer = item.First().Value;
                    timer.Stop();

                    _engine.Execute(string.Format("javascriptHelper.Escrever('deveria ter dispado: ' + stFunctionsCallBack[{0}]);", id));
                    _engine.Execute(string.Format("stFunctionsCallBack[{0}]();", id));


                }
            }
            finally
            {
                Monitor.Exit(_timerLock);
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
