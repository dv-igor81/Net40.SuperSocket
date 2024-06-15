using System;
using System.Net.Net40;

namespace SuperSocket.Udp
{
    class IPAddressUdpSessionIdentifierProvider : IUdpSessionIdentifierProvider
    {
        public string GetSessionIdentifier(IPEndPoint remoteEndPoint, ArraySegment<byte> data)
        {
            return remoteEndPoint.Address.ToString() + ":" + remoteEndPoint.Port;
        }
    }
}