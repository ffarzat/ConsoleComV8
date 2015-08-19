using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes.Expressions
{

    /// <summary>
    /// An assignment operator expression.
    /// </summary>
    public class AssignmentExpression: Expression
    {
        public AssignmentExpression Operator { get; set; }

        /// <summary>
        /// Pattern | Expression;
        /// </summary>
        private Node _left;

        public Expression Rigth { get; set; }

        /// <summary>
        /// Left como Pattern
        /// </summary>
        /// <param name="leftNode"></param>
        public AssignmentExpression(Pattern leftNode)
        {
            _left = leftNode; 
        }

        /// <summary>
        /// Left como Expression
        /// </summary>
        /// <param name="leftNode"></param>
        public AssignmentExpression(Expression leftNode)
        {
            _left = leftNode;
        }


    }
}
