using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class AsyncExtensions
    {
        /// <summary>
        /// Turns the given asynchronous functions into an IObservable
        /// </summary>
        public static IObservable<T> ToObservable<T>(
            this Func<Task<T>> getAsync,
            Func<Task<bool>> shouldLoopAsync)
        {
            return Observable.Create<T>(
                async observer =>
                {
                    while (await shouldLoopAsync())
                    {
                        var value = await getAsync();
                        observer.OnNext(value);
                    }
                    observer.OnCompleted();
                }
            );
        }
    }
}
