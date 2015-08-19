using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBureau;
using NUnit.Framework;
using Otimizacao.EsprimaAST;
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

        /// <summary>
        /// Testa se os Types ficam corretos nos sub tipos
        /// </summary>
        [Test]
        public void Construtor_Coloca_Type_Corretamente_Em_Nos_Filhos()
        {
            var programa = new Program();
            Assert.AreEqual(programa.Type, "Program");

            var emptyStatement = new EmptyStatement();
            Assert.AreEqual(emptyStatement.Type, "EmptyStatement");

        }

        /// <summary>
        /// Verifica se os valores da Enum UpdateOperator estão corretos
        /// </summary>
        [Test]
        public void ValidarEnunUpdateOperator()
        {
            string textoEnum = @"""++"" | ""--"" | ";
            var enumType = typeof(UpdateOperator);
            var valorTextoEnum = RetornarStringEnum(enumType);
            Assert.AreEqual(valorTextoEnum, textoEnum);
        }

        /// <summary>
        /// Verifica se os valores da Enum AssignmentOperator estão corretos
        /// </summary>
        [Test]
        public void ValidarEnunAssignmentOperator()
        {
            string textoEnum = @"""="" | ""+="" | ""-="" | ""*="" | ""/="" | ""%="" | ""<<="" | "">>="" | "">>>="" | ""|="" | ""^="" | ""&="" | ";
            var enumType = typeof(AssignmentOperator);
            var valorTextoEnum = RetornarStringEnum(enumType);
            Assert.AreEqual(valorTextoEnum, textoEnum);
        }

        /// <summary>
        /// Verifica se os valores da Enum LogicalOperator estão corretos
        /// </summary>
        [Test]
        public void ValidarEnunLogicalOperator()
        {
            string textoEnum = @"""||"" | ""&&"" | ";
            var enumType = typeof(LogicalOperator);
            var valorTextoEnum = RetornarStringEnum(enumType);
            Assert.AreEqual(valorTextoEnum, textoEnum);
        }

        /// <summary>
        /// Verifica se os valores da Enum BinaryOperator estão corretos
        /// </summary>
        [Test]
        public void ValidarEnunBinaryOperator()
        {
            string textoEnum = @"""=="" | ""!="" | ""==="" | ""!=="" | ""<"" | ""<="" | "">"" | "">="" | ""<<"" | "">>"" | "">>>"" | ""+"" | ""-"" | ""*"" | ""/"" | ""%"" | ""|"" | ""^"" | ""&"" | ""in"" | ""instanceof"" | ";
            var enumType = typeof(BinaryOperator);
            var valorTextoEnum = RetornarStringEnum(enumType);
            Assert.AreEqual(valorTextoEnum, textoEnum);
        }

        /// <summary>
        /// Verifica se os valores da Enum UnaryOperator estão corretos
        /// </summary>
        [Test]
        public void ValidarEnunUnaryOperator()
        {
            string textoEnum = @"""-"" | ""+"" | ""!"" | ""~"" | ""typeof"" | ""void"" | ""delete"" | ";
            var enumType = typeof(UnaryOperator);
            var valorTextoEnum = RetornarStringEnum(enumType);
            Assert.AreEqual(valorTextoEnum, textoEnum);
        }

        /// <summary>
        /// Determina a string exata de uma enum para comparação
        /// </summary>
        /// <param name="tipoEnum"></param>
        /// <returns></returns>
        private static string RetornarStringEnum(Type tipoEnum)
        {
            var enumInstance = new StringEnum(tipoEnum);
            var textoEnum = "";
            foreach (var stringValue in enumInstance.GetStringValues())
            {
                textoEnum += string.Format(@"""{0}"" | ", stringValue);
            }
            return textoEnum;
        }
    }
}
