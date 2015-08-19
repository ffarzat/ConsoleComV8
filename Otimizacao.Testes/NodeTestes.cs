using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Otimizacao.EsprimaAST.Nodes;
using Otimizacao.EsprimaAST.Nodes.Statements;

namespace Otimizacao.Testes
{
    /// <summary>
    /// Testes da classe Node
    /// </summary>
    [TestFixture]
    public class NodeTestes
    {
        [Test]
        public void Construtor_Coloca_Type_Corretamente_Em_Nos_Filhos()
        {
            var programa = new Program();
            Assert.AreEqual(programa.Type, "Program");

            var emptyStatement = new EmptyStatement();
            Assert.AreEqual(emptyStatement.Type, "EmptyStatement");

        }
    }
}
