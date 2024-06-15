using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;

namespace SuperSocket.Udp
{
    public class UdpConnectionFactory : IConnectionFactory
    {
        public Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
        {
            var connectionInfo = (UdpConnectionInfo)connection;

            return TaskEx.FromResult<IConnection>(new UdpPipeConnection(connectionInfo.Socket,
                connectionInfo.ConnectionOptions, connectionInfo.RemoteEndPoint, connectionInfo.SessionIdentifier));
        }
    }
}