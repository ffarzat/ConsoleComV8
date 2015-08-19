using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBureau;

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

        /// <summary>
        /// Construtor default
        /// </summary>
        public Node()
        {
            this.Type = this.GetType().Name;
        }
    }

    #region Enuns

    /// <summary>
    /// A unary operator token.
    /// </summary>
    /// <remarks>
    /// "-" | "+" | "!" | "~" | "typeof" | "void" | "delete"
    /// 
    /// https://developer.mozilla.org/pt-BR/docs/Web/JavaScript/Guide/Expressions_and_Operators
    /// </remarks>
    public enum UnaryOperator
    {
        [StringValue("-")]
        unaryNot = 0,
        [StringValue("+")]
        plus = 1,
        [StringValue("!")]
        notEqual = 2,
        [StringValue("~")]
        not = 3,
        [StringValue("typeof")]
        unaryTypeof = 4,
        [StringValue("void")]
        unaryVoid = 5,
        [StringValue("delete")]
        unaryDelete = 5,

    }

    /// <summary>
    /// A binary operator token.
    /// </summary>
    /// <remarks>
    ///  "==" | "!=" | "===" | "!==" | "<" | "<=" | ">" | ">=" | "<<" | ">>" | ">>>" | "+" | "-" | "*" | "/" | "%" | "|" | "^" | "&" | "in" | "instanceof"
    ///  https://developer.mozilla.org/pt-BR/docs/Web/JavaScript/Guide/Expressions_and_Operators
    /// </remarks>
    public enum BinaryOperator
    {
        [StringValue("==")]
        equalEqual = 0,

        [StringValue("!=")]
        diff = 1,
        
        [StringValue("===")]
        equalEqualEqual = 2,
        
        [StringValue("!==")]
        diffEqualEqual = 3,
        
        [StringValue("<")]
        less = 4,
        
        [StringValue("<=")]
        lessEqual = 5,
        
        [StringValue(">")]
        grater = 6,
        
        [StringValue(">=")]
        greaterEqual = 7,
        
        [StringValue("<<")]
        lessLess = 8,
        
        [StringValue(">>")]
        greaterGreater = 9,
        
        [StringValue(">>>")]
        greaterGreaterGreater = 10,
        
        [StringValue("+")]
        plus = 11,
        
        [StringValue("-")]
        minus = 12,
        
        [StringValue("*")]
        mult = 13,
        
        [StringValue("/")]
        div = 14,
        
        [StringValue("%")]
        mod = 15,
        
        [StringValue("|")]
        Or = 16,
        
        [StringValue("^")]
        pow = 17,
        
        [StringValue("&")]
        and = 18,
        
        [StringValue("in")]
        iN = 19,
        
        [StringValue("instanceof")]
        instanceOf = 20,
    }

    /// <summary>
    /// A logical operator token.
    /// </summary>
    /// <remarks>
    ///  "||" | "&&"
    /// https://developer.mozilla.org/pt-BR/docs/Web/JavaScript/Guide/Expressions_and_Operators
    /// </remarks>
    public enum LogicalOperator
    {
        [StringValue("||")]
        Or = 0,

        [StringValue("&&")]
        And = 1,
    }

    /// <summary>
    /// An assignment operator token.
    /// </summary>
    /// <remarks>
    /// "=" | "+=" | "-=" | "*=" | "/=" | "%=" | "<<=" | ">>=" | ">>>=" | "|=" | "^=" | "&="
    /// https://developer.mozilla.org/pt-BR/docs/Web/JavaScript/Guide/Expressions_and_Operators
    /// </remarks>
    public enum AssignmentOperator
    {
        [StringValue("=")]
        Set = 0,

        [StringValue("+=")]
        PlusSet = 1,

        [StringValue("-=")]
        MinusSet = 2,

        [StringValue("*=")]
        MultSet = 3,

        [StringValue("/=")]
        DivideSet = 4,

        [StringValue("%=")]
        ModSet = 5,

        [StringValue("<<=")]
        LeftSet = 6,

        [StringValue(">>=")]
        RigthSet = 7,

        [StringValue(">>>=")]
        RigthRigthSet = 8,

        [StringValue("|=")]
        OrSet = 9,

        [StringValue("^=")]
        PowSet = 10,

        [StringValue("&=")]
        AndSet = 11,
    }

    /// <summary>
    /// An update (increment or decrement) operator token.
    /// </summary>
    /// <remarks>
    /// "++" | "--"
    /// https://developer.mozilla.org/pt-BR/docs/Web/JavaScript/Guide/Expressions_and_Operators
    /// </remarks>
    public enum UpdateOperator
    {
        [StringValue("++")]
        Increment = 0,

        [StringValue("--")]
        Decrement = 1,
    }

    #endregion

}
