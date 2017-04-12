using System;
using System.Net;
using System.Threading.Tasks;
using ZipkinTracer.Models;

namespace ZipkinTracer.Internal
{
    internal interface IServiceEndpoint
    {
        Task<Endpoint> GetLocalEndpoint(string serviceName, IPAddress localIP, ushort port);
        Task<Endpoint> GetRemoteEndpoint(string serviceName, Uri serviceUri);
    }
}