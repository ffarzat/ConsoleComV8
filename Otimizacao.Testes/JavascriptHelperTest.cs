using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.ClearScript.V8;
using NUnit.Framework;
using Otimizacao.Javascript;
using Timer = System.Timers.Timer;

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
        /// Para os testes do timeout
        /// </summary>
        public string Nome { get; set; }

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
        public void ExecutarTestesDoLodash()
        {
            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, "Lodash"), true, true);

            helper.ExecutarTestes("lodash.js", "lodashTest.js");

            helper.FalhasDosTestes.ForEach(Console.WriteLine);

            Assert.AreEqual(0, helper.TestesComFalha, "Não deveria ter falhado nenhum dos testes");
            Assert.Greater(helper.TestesComSucesso, 1);
            
        }

        /// <summary>
        /// Executa os testes do underscore
        /// </summary>
        [Test]
        public void ExecutarTestesDounderscore()
        {
            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, "underscore"), true, true);

            helper.ExecutarTestes("underscore.js", "underscoreTests.js");

            helper.FalhasDosTestes.ForEach(Console.WriteLine);

            Assert.AreEqual(0, helper.TestesComFalha, "Não deveria ter falhado nenhum dos testes");
            Assert.Greater(helper.TestesComSucesso, 1);

        }
    }
}
