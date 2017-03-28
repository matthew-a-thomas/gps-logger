using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class TaskExtensions
    {
        public static T WaitAndGet<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}
