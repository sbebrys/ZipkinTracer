using System;
using Newtonsoft.Json;

namespace ZipkinTracer.Models.Serialization.Json
{
    internal class JsonEndpoint
    {
        private readonly Endpoint _endpoint;

        [JsonProperty("ipv4")]
        public string IPv4 => _endpoint.IPAddress.ToString();

        [JsonProperty("port")]
        public ushort Port => _endpoint.Port;

        [JsonProperty("serviceName")]
        public string ServiceName => _endpoint.ServiceName;

        public JsonEndpoint(Endpoint endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            _endpoint = endpoint;
        }
    }
}