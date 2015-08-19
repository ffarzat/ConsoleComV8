using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST
{
    /// <summary>
    /// Representa de nó na Árvore gerada pelo Esprima
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// Tipo. Pode conter os seguintes valores [definidos em https://github.com/estree/estree/blob/master/spec.md]:
        /// 
        /// Programs
        /// Functions
        /// Statements
        /// EmptyStatement
        /// BlockStatement
        /// ExpressionStatement
        /// IfStatement
        /// LabeledStatement
        /// BreakStatement
        /// ContinueStatement
        /// WithStatement
        /// ReturnStatement
        /// ThrowStatement
        /// TryStatement    
        /// WhileStatement
        /// DoWhileStatement
        /// ForStatement
        /// ForInStatement
        /// DebuggerStatement
        /// Declarations
        /// FunctionDeclaration
        /// VariableDeclaration
        /// VariableDeclarator
        /// Expressions
        /// ThisExpression
        /// ArrayExpression
        /// ObjectExpression
        /// Property
        /// FunctionExpression
        /// SequenceExpression
        /// UnaryExpression
        /// BinaryExpression
        /// AssignmentExpression
        /// UpdateExpression    
        /// LogicalExpression   
        /// ConditionalExpression
        /// CallExpression
        /// NewExpression
        /// MemberExpression
        /// Patterns            <remarks> Estranho esse aqui</remarks>
        /// SwitchCase
        /// CatchClause
        /// Identifier
        /// Literal
        /// RegExpLiteral
        /// UnaryOperator
        /// BinaryOperator
        /// LogicalOperator
        /// AssignmentOperator
        /// UpdateOperator
        /// </summary>
        public string Type { get; internal set; }

        /// <summary>
        /// Representa a localização do nó no código fonte. Pode ficar com valor nulo
        /// </summary>
        public SourceLocation Loc { get; set; } 
    }
}
