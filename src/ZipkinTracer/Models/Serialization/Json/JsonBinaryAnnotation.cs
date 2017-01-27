using System;
using Newtonsoft.Json;

namespace ZipkinTracer.Models.Serialization.Json
{
    internal class JsonBinaryAnnotation
    {
        private readonly BinaryAnnotation _binaryAnnotation;

        [JsonProperty("endpoint")]
        public JsonEndpoint Endpoint => new JsonEndpoint(_binaryAnnotation.Host);

        [JsonProperty("key")]
        public string Key => _binaryAnnotation.Key;

        [JsonProperty("value")]
        public string Value => _binaryAnnotation.Value.ToString();

        public JsonBinaryAnnotation(BinaryAnnotation binaryAnnotation)
        {
            if (binaryAnnotation == null)
                throw new ArgumentNullException(nameof(binaryAnnotation));

            _binaryAnnotation = binaryAnnotation;
        }
    }
}