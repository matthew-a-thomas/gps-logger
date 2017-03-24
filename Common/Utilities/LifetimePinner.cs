using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Common.Utilities
{
    /// <summary>
    /// Pins the lifetime of one thing to another.
    /// Note that two GC cycles are needed to finalize a thing after the thing it is pinned to is finalized
    /// </summary>
    public class LifetimePinner<TThing, TTo>
        where TThing : class
        where TTo : class
    {
        private readonly ConditionalWeakTable<TTo, ConcurrentBag<TThing>> _pinnings = new ConditionalWeakTable<TTo, ConcurrentBag<TThing>>();
        
        /// <summary>
        /// Pin the lifetime of "thing" to "to", so that "thing" is not finalized at least until soon after "to".
        /// In practice, it takes a couple of GC cycles before "thing" gets cleaned up (assuming you don't have any strong references to "thing" anywhere else)
        /// </summary>
        /// <param name="thing"></param>
        /// <param name="to"></param>
        public void Pin(TThing thing, TTo to)
        {
            _pinnings.GetOrCreateValue(to).Add(thing);
        }
    }
}
