using System.Collections.Concurrent;

namespace Common.Extensions
{
    public static class ConcurrentDictionaryExtensions
    {
        /// <summary>
        /// Tries to remove the given key from this dictionary, returning a boolean.
        /// Wraps the native method without exposing the removed item
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            // ReSharper disable once UnusedVariable
            return dictionary.TryRemove(key, out TValue value);
        }
    }
}
