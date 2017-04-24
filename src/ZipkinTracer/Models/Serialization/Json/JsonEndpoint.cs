using System;
using Newtonsoft.Json;
using ZipkinTracer.Extensions;

namespace ZipkinTracer.Models.Serialization.Json
{
    internal class JsonEndpoint
    {
        private readonly Endpoint _endpoint;

        [JsonProperty("ipv4", NullValueHandling = NullValueHandling.Ignore)]
        public string IPv4 => _endpoint.IPAddress.ToIPV4Integer();

        [JsonProperty("ipv6", NullValueHandling =NullValueHandling.Ignore)]
        public string IPv6 => _endpoint.IPAddress.ToIPV6Bytes();

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