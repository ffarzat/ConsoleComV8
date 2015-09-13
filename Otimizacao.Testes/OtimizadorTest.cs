using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

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
        [Test]
        public void ValidarExcel()
        {
            var otimizador = new Otimizador(5, 2, 20, "Require", "ResultadosUnderscore");
            otimizador.UsarSetTimeout();

            var otimizou = otimizador.Otimizar("underscore.js", "underscoreTests.js");
        }

    }
}
