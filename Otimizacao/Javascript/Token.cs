using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.Javascript
{
    /// <summary>
    /// Representa um token no Javascript
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Tipo do Token
        /// </summary>
        /// <remarks>
        /// Tipos possíveis
        ///     Boolean
        ///     Identifier
        ///     Keyword
        ///     Null
        ///     Numeric
        ///     Punctuator
        ///     RegularExpression
        ///     String
        /// </remarks>
        public string Tipo { get; set; }

        /// <summary>
        /// Valor do Token
        /// </summary>
        public String Valor { get; set; }
    }
}
