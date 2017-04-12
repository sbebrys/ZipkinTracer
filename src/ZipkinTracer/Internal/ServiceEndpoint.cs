using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ZipkinTracer.Models;

namespace ZipkinTracer.Internal
{
    internal class ServiceEndpoint : IServiceEndpoint
    {
        public async Task<Endpoint> GetLocalEndpoint(string serviceName, IPAddress localIP, ushort port)
        {
            return new Endpoint
            {
                ServiceName = serviceName,
                IPAddress = await GetLocalIPAddress() ?? localIP,
                Port = port
            };
        }

        public async Task<Endpoint> GetRemoteEndpoint(string serviceName, Uri serviceUri)
        {
            return new Endpoint
            {
                ServiceName = serviceName,
                IPAddress = await GetRemoteIPAddress(serviceUri),
                Port = (ushort)serviceUri.Port
            };
        }

        private async Task<IPAddress> GetLocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            try
            {
                var host = await Dns.GetHostEntryAsync(Dns.GetHostName());

                return host
                    .AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<IPAddress> GetRemoteIPAddress(Uri uri)
        {
            var adressList = await Dns.GetHostAddressesAsync(uri.Host);
            return adressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}