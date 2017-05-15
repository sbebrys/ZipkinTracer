using System;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using ZipkinTracer.Internal;

namespace ZipkinTracer.Test
{
    [TestFixture]
    public class ZipkinEndpointTests
    {

        [Test]
        public async Task GetLocalEndpoint()
        {
            var serviceName = "name";
            ushort port = 12312;

            var zipkinEndpoint = new ServiceEndpoint();
            var endpoint = await zipkinEndpoint.GetLocalEndpoint(serviceName, IPAddress.Loopback, port);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual(serviceName, endpoint.ServiceName);
            Assert.IsNotNull(endpoint.IPAddress);
            Assert.IsNotNull(endpoint.Port);
        }

        [Test]
        public async Task GetRemoteEndpoint()
        {
            var remoteUri = new Uri("http://localhost");
            var serviceName = "name";

            var zipkinEndpoint = new ServiceEndpoint();
            var endpoint = await zipkinEndpoint.GetRemoteEndpoint(serviceName, remoteUri);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual(serviceName, endpoint.ServiceName);
            Assert.IsNotNull(endpoint.IPAddress);
            Assert.IsNotNull(endpoint.Port);
        }
    }
}
