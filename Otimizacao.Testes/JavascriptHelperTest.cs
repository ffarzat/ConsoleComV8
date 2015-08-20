using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
