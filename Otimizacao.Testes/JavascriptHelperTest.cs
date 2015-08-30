﻿using System;
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
        /// Executa os testes de geracao de codigo
        /// </summary>
        [Test]
        public void GerarCodigo()
        {
            const string diretorioExecucao = "Require";
            //var scriptCode = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, diretorioExecucao, "global.js")).Replace("\n\n", "").Replace("\n", "").Replace(" ", "");
            var astMoment = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, diretorioExecucao, "astMoment.txt"));

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
            var scriptCode = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, diretorioExecucao, "global.js"));
            var astMoment = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, diretorioExecucao, "astMoment.txt"));

            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, diretorioExecucao), false, false);
            helper.ConfigurarGeracao();
            var ast = helper.GerarAst(scriptCode);

            Assert.AreEqual(astMoment, helper.JsonAst, "AST Inválida");
            Assert.AreEqual(ast, helper.JsonAst, "AST Inválida");
        }

        /// <summary>
        /// Executa os testes do Monent com SetTimeout and SetInterval ligadas
        /// </summary>
        [Test]
        public void ExecutarTestesDoMomentComSetTimeoutEInterval()
        {
            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, "Require"), true, true);
            helper.ConfigurarGeracao();
            helper.ExecutarTestes("global.js", "core-test.js");

            helper.FalhasDosTestes.ForEach(Console.WriteLine);
            Assert.AreEqual(0, helper.TestesComFalha, "Não deveria ter falhado nenhum dos testes");
            Assert.AreEqual(helper.TestesComSucesso, 57982);

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
        [Ignore]
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
        [Ignore]
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
        public void ExecutarMutacao()
        {
            var helper = new JavascriptHelper(Path.Combine(Environment.CurrentDirectory, "Require"), true, true);
            var scriptCode = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Require", "global.js"));
            helper.ConfigurarGeracao();
            var ast = helper.GerarAst(scriptCode);
            
            var astNova = helper.ExecutarMutacaoExclusao(ast);

            Assert.AreNotEqual(ast, astNova);
        }

    }
}
