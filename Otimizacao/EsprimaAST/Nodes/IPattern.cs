using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.EsprimaAST.Nodes
{

    /// <summary>
    /// Interface para um Pattern. 
    /// </summary>
    /// <remarks>
    /// Destructuring binding and assignment are not part of ES6, but all binding positions accept Pattern to allow for destructuring in ES6. 
    /// Nevertheless, for ES5, the only Pattern subtype is Identifier.
    /// </remarks>
    public interface IPattern
    {

    }
}
