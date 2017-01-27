using System;
using Newtonsoft.Json;

namespace ZipkinTracer.Models.Serialization.Json
{
    internal class JsonAnnotation
    {
        private readonly Annotation _annotation;

        [JsonProperty("endpoint")]
        public JsonEndpoint Endpoint => new JsonEndpoint(_annotation.Host);

        [JsonProperty("value")]
        public string Value => _annotation.Value;

        [JsonProperty("timestamp")]
        public long Timestamp => _annotation.Timestamp.ToUnixTimeMicroseconds();

        public JsonAnnotation(Annotation annotation)
        {
            if (annotation == null)
                throw new ArgumentNullException(nameof(annotation));

            _annotation = annotation;
        }
    }
}