using System;
using System.Threading.Tasks;
using Common.Errors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GPSLogger.Utilities
{
    public class ActionResultProducer : IActionResultProducer
    {
        private readonly IErrorHandler _errorHandler;

        public ActionResultProducer(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public async Task<IActionResult> ProduceAsync(Func<Task> fromAsync)
        {
            try
            {
                await fromAsync();
                return new OkResult();
            }
            catch (Exception e)
            {
                var response = _errorHandler.Handle(e);
                var serialized = JsonConvert.SerializeObject(response);
                return new ObjectResult(serialized)
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> ProduceAsync<T>(Func<Task<T>> fromAsync)
        {
            try
            {
                var response = await fromAsync();
                return new OkObjectResult(response);
            }
            catch (Exception e)
            {
                var response = _errorHandler.Handle(e);
                return new ObjectResult(response)
                {
                    StatusCode = 500
                };
            }
        }
    }
}
