using System;
using System.Threading.Tasks;
using ZipkinTracer.Models;

namespace ZipkinTracer.Internal
{
    internal interface IServiceEndpoint
    {
        Task<Endpoint> GetLocalEndpoint(string serviceName, ushort port);
        Task<Endpoint> GetRemoteEndpoint(Uri remoteServer, string remoteServiceName);
    }
}