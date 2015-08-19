using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{
    /// <summary>
    /// Representa um Statement com nome
    /// </summary>
    public class LabeledStatement: Statement
    {
        /// <summary>
        /// Identificador [label, texto]
        /// </summary>
        public Identifier Label { get; set; }

        /// <summary>
        /// Representa o Corpo do Statement
        /// </summary>
        public Statement Body { get; set; }
    }
}
