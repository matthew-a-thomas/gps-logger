using System;
using System.Threading;

namespace Common.Extensions
{
    public static class ActionExtensions
    {
        /// <summary>
        /// Creates a new action that will invoke the given action only up to once
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Action MakeSingular(this Action action)
        {
            if (action == null)
                return null;
            var numInvocations = 0;
            return () =>
            {
                if (Interlocked.Increment(ref numInvocations) != 1) return;
                action();
            };
        }
    }
}
