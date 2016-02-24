using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace Otimizacao.Testes
{
    /// <summary>
    /// Testes do otimzador
    /// </summary>
    [TestFixture]
    public class OtimizadorTest
    {

        /// <summary>
        /// Limpa os resultados de antes?
        /// </summary>
        [Test]
        public void LimparResultadosAnteriores()
        {
            var otimizador = new Otimizador(1, 1, 1, "Require", "ResultadosMoment");
            var otimizou = otimizador.Otimizar("global.js", "core-test.js");

            otimizador.LimparResultadosAnteriores();

            Assert.IsTrue(Directory.Exists("ResultadosMoment"));
            Assert.AreEqual(Directory.EnumerateFiles("ResultadosMoment").Count() , 0 );
        }

        /// <summary>
        /// Roda, recupera excel e verifica os valores
        /// </summary>
        [Test, Ignore]
        public void ValidarExcel()
        {
            var otimizador = new Otimizador(5, 2, 20, "Require", "ResultadosUnderscore");
            otimizador.ConfigurarRodada(1);
            otimizador.LimparResultadosAnteriores();
            otimizador.UsarSetTimeout();

            var otimizou = otimizador.Otimizar("underscore.js", "underscoreTests.js");
        }


        /// <summary>
        /// Roda, recupera excel e verifica os valores
        /// </summary>
        [Test]
        public void ValidarCsv()
        {
            var otimizador = new Otimizador(5, 2, 20, "Require", "ResultadosUnderscore");
            otimizador.ConfigurarRodada(2);
            otimizador.LimparResultadosAnteriores();
            otimizador.UsarSetTimeout();

            var otimizou = otimizador.Otimizar("underscore.js", "underscoreTests.js");
        }

        /// <summary>
        /// Testa a função que determina qual a function interna mais usada
        /// </summary>
        [Test]
        public void DeterminarFuncaoMaisUsadaTest()
        {
            var otimizador = new Otimizador(1, 1, 1, "Require", "ResultadosMoment");
            var otimizou = otimizador.Otimizar("global.js", "core-test.js");

            var lista = otimizador.DeterminarListaDeFuncoes(otimizador.MelhorIndividuo);

            Assert.AreEqual(141, lista.Count);
        }

        /// <summary>
        /// Testa se realmente executa mutação em uma ast simples de função
        /// </summary>
        [Test]
        public void ExecutarMutacaoNaFuncaoTest()
        {

            var otimizador = new Otimizador(1, 1, 1, "Require", "ResultadosMoment");
            var otimizou = otimizador.Otimizar("global.js", "core-test.js");

            var ast = otimizador.DeterminarListaDeFuncoes(otimizador.MelhorIndividuo)[1].Ast;
            var novaAst = otimizador.ExecutarMutacaoNaFuncao(ast, 10);

            Assert.AreNotEqual(ast, novaAst);

            File.WriteAllText("astFuncao.txt", JToken.Parse(ast).ToString());
            File.WriteAllText("astNovaFuncao.txt", JToken.Parse(novaAst).ToString());

        }

        /// <summary>
        /// Testa de mutacao dentro de funcao específica
        /// </summary>
        [Test]
        public void AtualizarFuncaoTest()
        {
            var otimizador = new Otimizador(1, 1, 1, "Require", "ResultadosMoment");
            var otimizou = otimizador.Otimizar("global.js", "core-test.js");

            var funcao = otimizador.DeterminarListaDeFuncoes(otimizador.MelhorIndividuo)[1];
            var novaAst = otimizador.ExecutarMutacaoNaFuncao(funcao.Ast, 25);

            Assert.AreNotEqual(funcao.Ast, novaAst);

            File.WriteAllText("astFuncao.txt", JToken.Parse(funcao.Ast).ToString());
            File.WriteAllText("astNovaFuncao.txt", JToken.Parse(novaAst).ToString());


            string novaAstIndividuo = otimizador.AtualizarFuncao(otimizador.MelhorIndividuo, funcao.Nome, novaAst);

            Assert.AreNotEqual(otimizador.MelhorIndividuo.Ast, novaAstIndividuo);

            File.WriteAllText("astIndividuo.txt", JToken.Parse(otimizador.MelhorIndividuo.Ast).ToString());
            File.WriteAllText("astNovoIndividuo.txt", JToken.Parse(novaAstIndividuo).ToString());

            var c = otimizador.MelhorIndividuo.Clone();
            c.Ast = novaAstIndividuo;
            otimizador.GerarCodigo(c);

            otimizador.GerarCodigo(otimizador.MelhorIndividuo);


        }


    }
}
