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
        public async Task<Endpoint> GetLocalEndpoint(string serviceName, ushort port)
        {
            return new Endpoint
            {
                ServiceName = serviceName,
                IPAddress = await GetLocalIPAddress(),
                Port = port
            };
        }

        public async Task<Endpoint> GetRemoteEndpoint(Uri remoteServer, string remoteServiceName)
        {
            var address = await GetRemoteIPAddress(remoteServer);
            var addressBytes = address.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(addressBytes);
            }

            var ipAddressStr = BitConverter.ToInt32(addressBytes, 0);
            var hostIPAddressStr = IPAddress.HostToNetworkOrder(ipAddressStr);

            return new Endpoint()
            {
                ServiceName = remoteServiceName,
                IPAddress = await GetRemoteIPAddress(remoteServer),
                Port = (ushort) remoteServer.Port
            };
        }

        private async Task<IPAddress> GetLocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = await Dns.GetHostEntryAsync(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        private async Task<IPAddress> GetRemoteIPAddress(Uri remoteServer)
        {
            var adressList = await Dns.GetHostAddressesAsync(remoteServer.Host);
            return adressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}