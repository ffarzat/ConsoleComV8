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

            Assert.IsFalse(Directory.Exists("ResultadosMoment"));
        }

    }
}
