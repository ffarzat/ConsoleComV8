using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Otimizacao.EsprimaAST.Nodes;
using Otimizacao.EsprimaAST.Nodes.Expressions;
using Otimizacao.EsprimaAST.Nodes.Statements;
using ExpressionStatement = Otimizacao.EsprimaAST.Nodes.ExpressionStatement;

namespace Otimizacao.EsprimaAST
{
   /// <summary>
   /// Fábrica de Nós da Árvore do Esprima
   /// </summary>
    public class Fabrica
    {
        /// <summary>
        /// Dado um objeto em Json com a AST constrói os nós específicos
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        public Node Construir(JObject jsonObject)
        {
            return PreencherNoPrograma(jsonObject);
        }

        /// <summary>
        /// Cria o nó Program
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        private Node PreencherNoPrograma(JObject jsonObject)
        {
            #region Dbc
            if(!PropriedadeExiste("type", jsonObject))
               throw new ArgumentOutOfRangeException("jsonObject", "Type não definido");
            
            if(ValorDaPropriedadeComoString("type", jsonObject) != "Program")
               throw new ArgumentOutOfRangeException("jsonObject", string.Format("Type inválido {0}", ValorDaPropriedadeComoString("type", jsonObject)));
            #endregion
            
            var noProcessado = new Program();
            noProcessado.Body = PreencherListaStatements(jsonObject["body"]);

            return noProcessado;
        }

        /// <summary>
        /// Cria o nó Body do Program
        /// </summary>
        /// <param name="jToken"></param>
        /// <returns></returns>
        private List<Statement> PreencherListaStatements(JToken jToken)
        {
            return jToken.Select(PreencherStatement).ToList();
        }

       /// <summary>
        /// Cria o Statement
        /// </summary>
        /// <param name="jToken"></param>
        /// <returns></returns>
        private Statement PreencherStatement(JToken jToken)
       {
           var statement = new Statement();
           var stType = ValorDaPropriedadeComoString("type", jToken as JObject);
           switch (stType)
           {
               case "BlockStatement" :
                   statement = new BlockStatement();
                   break;
               case "EmptyStatement":
                   statement = new EmptyStatement();
                   break;
               case "ExpressionStatement":
                   statement = new ExpressionStatement()
                       {
                           Expression = PreencherExpressao(jToken["Expression"])
                       };
                   break;
           }

           return statement;
       }

        /// <summary>
        /// Cria as Expressões
        /// </summary>
        /// <param name="jToken"></param>
        /// <returns></returns>
       private Expression PreencherExpressao(JToken jToken)
       {
           var stType = ValorDaPropriedadeComoString("type", jToken as JObject);
           Node retorno = null;

           switch (stType)
           {
               case "CallExpression":
                   retorno = new CallExpression();
                   break;
           }

            return (Expression) retorno;
       }


       /// <summary>
        /// Determina se o campo existe
        /// </summary>
        /// <param name="nome"></param>
        /// <param name="jObject"></param>
        /// <returns></returns>
        private bool PropriedadeExiste(string nome, JObject jObject)
        {
            return jObject[nome] != null;
        }

        /// <summary>
        /// Recupera o valor de uma propriedade como string
        /// </summary>
        /// <param name="nome"></param>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        private string ValorDaPropriedadeComoString(string nome, JObject jsonObject)
        {
            return jsonObject[nome].ToString();
        }
    }
}
