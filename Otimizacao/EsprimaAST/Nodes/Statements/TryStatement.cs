using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otimizacao.EsprimaAST.Nodes.Clauses;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{
    /// <summary>
    /// A try statement. If handler is null then finalizer must be a BlockStatement.
    /// </summary>
    public class TryStatement: Statement
    {
        /// <summary>
        /// Bloco do Try
        /// </summary>
        public BlockStatement Block { get; set; }

        /// <summary>
        /// Catch
        /// </summary>
        public CatchClause Hanlder { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BlockStatement Finalizer { get; set; }
    }
}
