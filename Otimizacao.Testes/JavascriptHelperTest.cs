using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ClearScript.V8;
using NUnit.Framework;
using Otimizacao.Javascript;

namespace Otimizacao.Testes
{

    /// <summary>
    /// Testes do Helper para trabalhar com os arquivos em javascript na Engine V8
    /// </summary>
    [TestFixture]
    public class JavascriptHelperTest
    {

        /// <summary>
        /// Engine do v8 para testes simultaneos
        /// </summary>
        private V8ScriptEngine _engine;

        /// <summary>
        /// Para o teste do timeout
        /// </summary>
        public int Global { get; set; }

        /// <summary>
        /// Executa os testes do MomentJs
        /// </summary>
        [Test]
        public void ExecutarTestesDoMoment()
        {
            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, "Moment"));

            helper.ExecutarTestes("global.js", "core-test.js");

            helper.FalhasDosTestes.ForEach(Console.WriteLine);
            Assert.AreEqual(0, helper.TestesComFalha, "Não deveria ter falhado nenhum dos testes");
            Assert.AreEqual(helper.TestesComSucesso, 57982);
        }

        /// <summary>
        /// Executa os testes do Loadsh
        /// </summary>
        [Test]
        public void ExecutarTestesDoLoadsh()
        {
            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, "Lodash"));

            helper.ExecutarTestes("lodash.js", "lodashTest.js");

            helper.FalhasDosTestes.ForEach(Console.WriteLine);
            Assert.AreEqual(0, helper.TestesComFalha, "Não deveria ter falhado nenhum dos testes");
            Assert.Greater(helper.TestesComSucesso, 1);
            
        }

        /// <summary>
        /// Testa se é possível executar a engine "ao mesmo tempo" para simular um timeout
        /// </summary>
        [Test]
        public void Testa_engine_assync()
        {
            var host = new JavascriptHelperTest();
            _engine= new V8ScriptEngine();
            _engine.AddHostType("Console", typeof(Console));
            _engine.AddHostObject("JavascriptHelperTest", host);
            _engine.Execute("JavascriptHelperTest.Global = 0;");

            SetTimeout(100, () => _engine.Execute("JavascriptHelperTest.Global = 1;"));

            //_engine.Execute("Console.WriteLine('valor:{0}', JavascriptHelperTest.Global);");
            
            Assert.AreEqual(0, host.Global);
            
            Thread.Sleep(200);

            Assert.AreEqual(1, host.Global);

        }

        /// <summary>
        /// Para emular o TSetTimeout
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        async void SetTimeout(int ms, Action callback)
        {
            var startTime = DateTime.UtcNow;
            
            while ((DateTime.UtcNow - startTime).TotalMilliseconds < ms)
            {
                await Task.Delay(10);
            }

            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            callback();
        }
    }
}
