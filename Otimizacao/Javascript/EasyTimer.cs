using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.Javascript
{
    /// <summary>
    /// Retirada de http://www.dailycoding.com/posts/easytimer__javascript_style_settimeout_and_setinterval_in_c.aspx
    /// 
    /// Simula um SetTimeout usando timers
    /// 
    /// </summary>
    public static class EasyTimer
    {
        /// <summary>
        /// Cria um intervalo de chamadas no c#
        /// </summary>
        /// <param name="method"></param>
        /// <param name="delayInMilliseconds"></param>
        /// <returns></returns>
        public static IDisposable SetInterval(Action method, int delayInMilliseconds)
        {
            var timer = new System.Timers.Timer(delayInMilliseconds);
            timer.Elapsed += (source, e) => method();

            timer.Enabled = true;
            timer.Start();

            // Returns a stop handle which can be used for stopping
            // the timer, if required
            return timer as IDisposable;
        }

        /// <summary>
        /// Agenda uma única chamada no c#
        /// </summary>
        /// <param name="method"></param>
        /// <param name="delayInMilliseconds"></param>
        /// <returns></returns>
        public static IDisposable SetTimeout(Action method, int delayInMilliseconds)
        {
            var timer = new System.Timers.Timer(delayInMilliseconds);
            timer.Elapsed += (source, e) => method();

            timer.AutoReset = false;
            timer.Enabled = true;
            timer.Start();

            // Returns a stop handle which can be used for stopping
            // the timer, if required
            return timer as IDisposable;
        }
    }
}
