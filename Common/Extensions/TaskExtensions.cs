using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Waits synchronously for this task to complete,
        /// then returns the task's result.
        /// Note that it's much more preferrable to switch to the async/await pattern than it is to use this method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static T WaitAndGet<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}
