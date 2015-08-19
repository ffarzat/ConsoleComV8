using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otimizacao.EsprimaAST.Nodes.Declarations;

namespace Otimizacao.EsprimaAST.Nodes.Statements
{

    /// <summary>
    /// A for/in statement.
    /// </summary>
    public class ForInStatement:Statement
    {
        /// <summary>
        /// VariableDeclaration |  Expression;
        /// </summary>
        private Node _left;

        public Expression Rigth { get; set; }

        public Statement Body { get; set; }

        /// <summary>
        /// left como VariableDeclaration
        /// </summary>
        /// <param name="node"></param>
        public ForInStatement(VariableDeclaration node)
        {
            _left = node; 
        }

        /// <summary>
        /// left como Expression
        /// </summary>
        /// <param name="node"></param>
        public ForInStatement(Expression node)
        {
            _left = node;
        }

        #region Privates

        /// <summary>
        /// Adiciona o left como VariableDeclaration
        /// </summary>
        /// <param name="node"></param>
        public void Left(VariableDeclaration node)
        {
            _left = node;
        }

        /// <summary>
        /// Adiciona o left como Expression
        /// </summary>
        /// <param name="node"></param>
        public void Left(Expression node)
        {
            _left = node;
        }

        #endregion
    }
}
