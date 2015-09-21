using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.ClearScript.V8;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        /// Executa os testes de geracao de codigo
        /// </summary>
        /// <remarks>Usado apenas para debugs</remarks>
        [Test]
        [Ignore]
        public void GerarCodigoDoMoment()
        {
            const string diretorioExecucao = "Require";
            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, diretorioExecucao), false, false);
            helper.ConfigurarGeracao();

            var scriptCode = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, diretorioExecucao, "global.js"));
            var astMoment = helper.GerarAst(scriptCode);

            File.WriteAllText("astMoment.txt", helper.FormatarStringJson(astMoment));
            
            var codigo = helper.GerarCodigo(astMoment);

            File.WriteAllText("codigo.txt", scriptCode);
            File.WriteAllText("codigoNovo.txt", codigo);

            Assert.AreEqual(scriptCode, helper.Codigo, "Código Inválido");
        }


        /// <summary>
        /// Executa os testes de geracao de codigo
        /// </summary>
        [Test]
        public void GerarCodigo()
        {
            const string diretorioExecucao = "Require";
            //var scriptCode = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, diretorioExecucao, "global.js")).Replace("\n\n", "").Replace("\n", "").Replace(" ", "");
            var astMoment = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, diretorioExecucao, "astUnderscore.txt"));

            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, diretorioExecucao), false, false);
            helper.ConfigurarGeracao();
            var codigo = helper.GerarCodigo(astMoment); //.Replace("\n\n", "").Replace("\n", "").Replace(" ", "");

            //Assert.AreEqual(scriptCode, codigo, "Código Inválido");
            Assert.AreEqual(codigo, helper.Codigo, "Código Inválido");
        }


        /// <summary>
        /// Executa os testes de geração da AST
        /// </summary>
        [Test]
        public void GerarAst()
        {
            const string diretorioExecucao = "Require";
            var scriptCode = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, diretorioExecucao, "underscore.js"));
            var astUnderscore = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, diretorioExecucao, "astUnderscore.txt"));

            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, diretorioExecucao), true, false);
            helper.ConfigurarGeracao();
            var ast = helper.GerarAst(scriptCode);

            File.WriteAllText("astUnderscoreNovo.txt", helper.FormatarStringJson(ast));

            Assert.AreEqual(astUnderscore, helper.FormatarStringJson(helper.JsonAst), "AST Inválida");
            Assert.AreEqual(ast, helper.JsonAst, "AST Inválida");
        }

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
            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, "Lodash"), true, false);

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

        /// <summary>
        /// Testa o procedimento de mutação (excluir um nó)
        /// </summary>
        [Test]
        public void ExecutarMutacaoExclusao()
        {
            var sw = new Stopwatch();
            sw.Start();

            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, "Require"), true, false);
            var scriptCode = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Require", "global.js"));
            helper.ConfigurarGeracao();

            var ast = helper.GerarAst(scriptCode);
            
            var astNova = helper.ExecutarMutacaoExclusao(ast, 175);

            sw.Stop();
            Console.WriteLine("ExecutarMutacaoExclusao {0}", sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff"));

            Assert.AreNotEqual(ast, astNova);

            File.WriteAllText("astOriginal.txt", helper.FormatarStringJson(ast));
            File.WriteAllText("astMutada.txt", helper.FormatarStringJson(astNova));

            
            sw.Reset();
            sw.Start();

            //var codigo = helper.GerarCodigo(ast);
            var codigoNovo = helper.GerarCodigo(astNova);

            sw.Stop();
            Console.WriteLine("GerarCodigo {0}", sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff"));

            Assert.AreNotEqual("", codigoNovo);
            Assert.AreNotEqual(scriptCode, codigoNovo);

            File.WriteAllText("codigo.txt", scriptCode);
            File.WriteAllText("codigoNovo.txt", codigoNovo);

        }

        [Test]
        public void ExecutarCrossOver()
        {
            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, "Require"), true, false);
            var scriptCode = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Require", "underscore.js"));
            helper.ConfigurarGeracao();

            var ast = helper.GerarAst(scriptCode);

            var astNova = helper.ExecutarMutacaoExclusao(ast, 100);

            Assert.AreNotEqual(ast, astNova);

            string astFilho1, astFilho2;

            helper.ExecutarCrossOver(ast, astNova, 348, 456, out astFilho1, out astFilho2);


            var codigo = helper.GerarCodigo(astFilho1);
            var codigoNovo = helper.GerarCodigo(astFilho2);

            Assert.AreNotEqual(codigo, codigoNovo);

            File.WriteAllText("codigo.txt", codigo);
            File.WriteAllText("codigoNovo.txt", codigoNovo);

        }

        /// <summary>
        /// Conta quantos nós existem na Ast
        /// </summary>
        [Test]
        public void ExecutarContagemDeNos()
        {
            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, "Require"), true, false);
            var scriptCode = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Require", "underscore.js"));
            helper.ConfigurarGeracao();

            var ast = helper.GerarAst(scriptCode);

            var total = helper.ContarNos(ast);

            Assert.AreNotEqual(0, total);
            Assert.AreEqual(6549, total);

        }

    }
}
