using System;
using System.Threading;

namespace Common.Utilities
{
    public static class RaceConditionBreaker
    {
        /// <summary>
        /// Repeatedly calls the given "loop" function until it returns false.
        /// Between each iteration, a small delay is introduced in a way that no two threads will always delay for the same amount of time.
        /// </summary>
        /// <param name="loop"></param>
        public static void BreakRaceCondition(Func<bool> loop)
        {
            for (var iteration = 1; loop(); ++iteration)
            {
                Thread.Sleep(Math.Abs(Thread.CurrentThread.GetHashCode() * iteration + 3) % 10);
            }
        }
    }
}
