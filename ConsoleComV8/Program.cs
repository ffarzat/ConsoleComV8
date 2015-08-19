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
using Newtonsoft.Json.Linq;
using Otimizacao.EsprimaAST.Json;
using Otimizacao.Javascript;

namespace ConsoleComV8
{
    class Program
    {
        static void Main(string[] args)
        {


            var sw = new Stopwatch();
            sw.Start();

            #region Ler arquivos Js
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
            #endregion

            #region Cria a Engine e configura com o JavascriptHelper e Console
            var engine = new V8ScriptEngine();
            var helper = new JavascriptHelper();
            engine.AddHostObject("javascriptHelper", helper);
            engine.Execute(console);
            engine.Execute("var console = new console();");
            #endregion

            #region Congigura o Escodegen e o Esprima
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
                                                        style: '\t'
                                                    },
                                                    quotes: 'auto'
                                                }
                                            };");


            var esprimaParse = string.Format(@"var syntax = esprima.parse({0}, {{ raw: true, tokens: true, range: true, comment: true }});", helper.EncodeJsString(scriptCode));
            engine.Execute(esprimaParse);
            engine.Execute("javascriptHelper.JsonAst = JSON.stringify(syntax);"); //Passo para o c#

            helper.Program = JsonConvert.DeserializeObject<Otimizacao.EsprimaAST.Nodes.Program>(helper.JsonAst, new EsprimaAstConverter());

            engine.Execute("syntax = ObjEscodegen.attachComments(syntax, syntax.comments, syntax.tokens);");
            engine.Execute("var code = ObjEscodegen.generate(syntax, option);");
            engine.Execute("javascriptHelper.Codigo = code;");

            #endregion

            #region Configura o QUnit

            engine.Execute(qunit);

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

            #region Carrega o individuo
            engine.Execute(helper.Codigo);
            #endregion

            #region Carrega e executa os Testes
            engine.Execute(scriptTestCode);

            engine.Execute(@"   QUnit.load();
                                QUnit.start();
                ");
            #endregion

            sw.Stop();
            
            Console.WriteLine("{0} segundos totais", sw.Elapsed.Seconds);
            Console.Read();

        }

    }





}
