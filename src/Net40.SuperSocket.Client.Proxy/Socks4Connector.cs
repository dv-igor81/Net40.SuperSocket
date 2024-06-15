using System;
using System.Net.Net40;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client.Proxy
{
    public class Socks4Connector : ConnectorBase
    {
        protected override ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}