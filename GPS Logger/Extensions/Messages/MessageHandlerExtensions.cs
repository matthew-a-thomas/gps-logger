using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GPS_Logger.Security.Messages;
using GPS_Logger.Serialization;

namespace GPS_Logger.Extensions.Messages
{
    public static class MessageHandlerExtensions
    {
        /// <summary>
        /// Creates a response as though an empty request was received by the client
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageHandler"></param>
        /// <param name="content"></param>
        /// <param name="contentSerializer"></param>
        /// <returns></returns>
        public static MessageToClient<T> CreateUnsignedResponse<T>(this MessageHandler messageHandler, T content, ISerializer<T> contentSerializer) => messageHandler.CreateResponse(new MessageFromClient<bool>(), Serializer<bool>.CreatePassthroughSerializer(), valid => content, contentSerializer);
    }
}