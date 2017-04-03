using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Utilities
{
    public interface IActionResultProducer
    {
        Task<IActionResult> ProduceAsync(Func<Task> fromAsync);
        Task<IActionResult> ProduceAsync<T>(Func<Task<T>> fromAsync);
    }
}
