using System;
using System.Net.Net40;

namespace SuperSocket.Udp
{
    public interface IUdpSessionIdentifierProvider
    {
        string GetSessionIdentifier(IPEndPoint remoteEndPoint, ArraySegment<byte> data);
    }
}