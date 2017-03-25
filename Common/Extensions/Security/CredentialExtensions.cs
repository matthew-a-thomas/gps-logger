using System;
using System.Threading.Tasks;
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
        /// <param name="conversionFunctionAsync"></param>
        /// <returns></returns>
        public static async ValueTask<Credential<TTo>> ConvertAsync<TFrom, TTo>(this Credential<TFrom> from,
            Func<TFrom, ValueTask<TTo>> conversionFunctionAsync) => new Credential<TTo>
        {
                ID = await conversionFunctionAsync(from.ID),
                Secret = await conversionFunctionAsync(from.Secret)
        };
    }
}