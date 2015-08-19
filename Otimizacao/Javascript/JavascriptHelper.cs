using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Otimizacao.EsprimaAST.Nodes;

namespace Otimizacao.Javascript
{
    /// <summary>
    /// Rápido Helper para Js
    /// </summary>
    public class JavascriptHelper
    {
        /// <summary>
        /// String com o Json da árvore do Javascript processada
        /// </summary>
        public string JsonAst { get; set; }

        /// <summary>
        /// Árvore tipada do Esprima
        /// </summary>
        public Program Program { get; set; }

        /// <summary>
        /// Codigo regerado pelo Escodegen
        /// </summary>
        public string Codigo { get; set; }

        /// <summary>
        /// NLog Logger
        /// </summary>
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Escrever(string arg, object args1 = null, object args2 = null)
        {
            var consoleOut = arg.Replace("%s", "{0}");
            string one = args1 == null ? "" : args1.ToString();
            string two = args2 == null ? "" : args2.ToString();
            _logger.Info(consoleOut, one, two);
        }

        /// <summary>
        /// Grava um Log do tipo Info direto no arquivo
        /// </summary>
        public static void Log(string mensagem)
        {
            _logger.Info(mensagem);
        }

        /// <summary>
        /// Encodes a string to be represented as a string literal. The format
        /// is essentially a JSON string.
        /// 
        /// The string returned includes outer quotes 
        /// Example Output: "Hello \"Rick\"!\r\nRock on"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string EncodeJsString(string s)
        {
            var sb = new StringBuilder();
            sb.Append("\"");
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");

            return sb.ToString();
        }
    }
}
