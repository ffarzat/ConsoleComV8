using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Otimizacao.EsprimaAST.Nodes;

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
        public Program Construir(JObject jsonObject)
        {
            return PreencherNoPrograma(jsonObject);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        private Program PreencherNoPrograma(JObject jsonObject)
        {
            #region Dbc
            if(!PropriedadeExiste("type", jsonObject))
               throw new ArgumentOutOfRangeException("jsonObject", "Type não definido");
            
            if(ValorDaPropriedadeComoString("type", jsonObject) != "Program")
               throw new ArgumentOutOfRangeException("jsonObject", string.Format("Type inválido {0}", ValorDaPropriedadeComoString("type", jsonObject)));
            #endregion

            var program = new Program();
            program.Body = ProcessarNo<Statement>(jsonObject);
            program.Loc = ProcessarNo<SourceLocation>(jsonObject);

            return program;
        }

        /// <summary>
        /// Cria o nó específico
        /// </summary>
        /// <typeparam name="T">Tipo do Nó</typeparam>
        /// <param name="jsonObject">Json com os valores para o nó</param>
        /// <returns></returns>
        private T ProcessarNo<T>(JObject jsonObject) where T : new()
        {
            var nodeType = ValorDaPropriedadeComoString("type", jsonObject);
            var noProcessado = new T();
            
            //switch (nodeType)
            //{
            //    case "Statement":
            //        noProcessado = CriarStatement(jsonObject);
            //        break;

            //}

            return noProcessado;
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
