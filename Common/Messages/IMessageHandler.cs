using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Security.Signing;

namespace Common.Messages
{
    public interface IMessageHandler<TRequest, TResponse>
    {
        ValueTask<SignedMessage<TResponse>> CreateResponseAsync(
            SignedMessage<TRequest> request,
            Func<bool, ValueTask<TResponse>> contentGeneratorAsync
        );
    }
}
