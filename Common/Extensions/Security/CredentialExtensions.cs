using System;
using Common.Security;

namespace Common.Extensions.Security
{
    public static class CredentialExtensions
    {
        /// <summary>
        /// Converts to another type of credential
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <param name="from"></param>
        /// <param name="conversionFunction"></param>
        /// <returns></returns>
        public static Credential<TTo> Convert<TFrom, TTo>(this Credential<TFrom> from,
            Func<TFrom, TTo> conversionFunction) => new Credential<TTo>
        {
                ID = conversionFunction(from.ID),
                Secret = conversionFunction(from.Secret)
        };
    }
}