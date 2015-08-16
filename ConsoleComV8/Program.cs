using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ClearScript.V8;
using NLog;
using Newtonsoft.Json;

namespace ConsoleComV8
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();

            var scriptCode = File.ReadAllText("global.js");
            var scriptTestCode = File.ReadAllText("core-test.js");
            var qunit = File.ReadAllText("qunit-1.18.0.js");
            var console = File.ReadAllText("Console.js");
            var esprima = File.ReadAllText("esprima.js");
            var escodegen = File.ReadAllText("escodegen.js");
            var estraverse = File.ReadAllText("estraverse.js");
            var ast = File.ReadAllText("ast.js");
            var code = File.ReadAllText("code.js");
            var keyword = File.ReadAllText("keyword.js");
            

            var engine = new V8ScriptEngine();
            engine.AddHostType("JavascriptHelper", typeof(JavascriptHelper));
            engine.Execute(console);
            engine.Execute("var console = new console();");
            
            //engine.Execute(require);
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
            
            
            //engine.Execute(entry_point);
            
            
            
            
            
            
            engine.Execute(esprima);
            
            
            //do the modification!
            engine.Execute(@"        option = {
                                                comment: true,
                                                format: {
                                                    indent: {
                                                        style: '\t'
                                                    },
                                                    quotes: 'auto'
                                                }
                                            };");


            var esprimaParse = string.Format(@"var syntax = esprima.parse({0}, {{ raw: true, tokens: true, range: true, comment: true }});", JavascriptHelper.EncodeJsString(scriptCode));
            engine.Execute(esprimaParse);
            engine.Execute("JavascriptHelper.Syntax = JSON.stringify(syntax);");
            
            
            //engine.Execute(string.Format(@"var syntax = esprima.parse({0}, {{ raw: true, tokens: true, range: true, comment: true }});", JavascriptHelper.EncodeJsString("var testeFabio = 34; //teste")));
            
            
            
            engine.Execute("syntax = ObjEscodegen.attachComments(syntax, syntax.comments, syntax.tokens);");
            engine.Execute("var code = ObjEscodegen.generate(syntax, option);");
            engine.Execute("JavascriptHelper.Codigo = code;");
            
            //end


            engine.Execute(qunit);


            #region registra os retornos dos testes
            engine.Execute(@"   
                                    var total, sucesso, falha;

                                    QUnit.done(function( details ) {
                                    console.log('=============================================');
                                    console.log('Total:' + details.total);
                                    console.log('Falha:' + details.failed);
                                    console.log('Sucesso:' + details.passed);
                                    console.log('Tempo:' + details.runtime);
                                       
                                    total = details.total;
                                    sucesso = details.passed;
                                    falha = details.failed;

                                });

/*

                                QUnit.testDone(function( details ) {
                                    if(details.failed > 0)
                                    {
                                        console.log('=============================================');
                                        console.log('Modulo:' + details.module);
                                        console.log('Teste:' + details.name);
                                        console.log(' Falha:' + details.failed);
                                        console.log(' Total:' + details.total);
                                        console.log(' Tempo:' + details.duration);
                                    }
                                });
*/

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

                                    console.log('=============================================');
                                    console.log( output );
                                });



                                QUnit.config.autostart = false;
                                QUnit.config.ignoreGlobalErrors = true;
                        ");
            #endregion



            engine.Execute(JavascriptHelper.Codigo);

            //engine.Execute("console.log('{0}', _);");

            engine.Execute(scriptTestCode);

            engine.Execute(@"   QUnit.load();
                                QUnit.start();
                ");


            sw.Stop();
            Console.WriteLine(sw.Elapsed.Seconds);
            Console.Read();

        }

    }

    public static class JavascriptHelper
    {

        public static string Syntax {get; set; }

        /// <summary>
        /// Codigo regerado pelo Escodegen
        /// </summary>
        public static string Codigo;

        /// <summary>
        /// NLog Logger
        /// </summary>
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Escrever(string arg, object args1 = null, object args2 = null)
        {
            var consoleOut = arg.Replace("%s", "{0}");
            string one = args1 == null ? "" : args1.ToString();
            string two = args2 == null ? "" : args2.ToString();
            _logger.Info(consoleOut, one, two);
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
        public static string EncodeJsString(string s)
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
    }




}
