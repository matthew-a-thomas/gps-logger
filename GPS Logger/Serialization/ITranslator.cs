using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GPS_Logger.Serialization
{
    /// <summary>
    /// Copies fields from one thing to another
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    public interface ITranslator<TFrom, TTo>
        where TTo : new()
    {
        /// <summary>
        /// Copies all the relevant fields from the given thing to a new instance of TTo
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        TTo Translate(TFrom thing);
    }
}