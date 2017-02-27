using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using GPS_Logger.Extensions;
using Newtonsoft.Json;

namespace GPS_Logger
{
    /// <summary>
    /// Makes responses to text/html requests format to JSON,
    /// and additionally serializes instances of IEnumerable&lt;byte&gt; as hex strings
    /// </summary>
    /// <remarks>Inspired by http://stackoverflow.com/a/20556625/3063273</remarks>
    public class BrowserJsonFormatter : JsonMediaTypeFormatter
    {
        /// <summary>
        /// Converts IEnumerables of bytes into hex strings
        /// </summary>
        private class ByteEnumerable : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var enumerable = value as IEnumerable<byte>;
                if (ReferenceEquals(enumerable, null)) throw new NotImplementedException();

                writer.WriteValue(enumerable.ToHexString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType) => typeof(IEnumerable<byte>).IsAssignableFrom(objectType);
        }

        public BrowserJsonFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            SerializerSettings.Formatting = Formatting.Indented;
            SerializerSettings.Converters.Add(new ByteEnumerable());
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue("application/json");
        }
    }
}