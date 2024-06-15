using System.IO;
using System.Net.Sockets.Net40;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    public class NetworkStreamInitializer : IConnectionStreamInitializer
    {
        public void Setup(ListenOptions listenOptions)
        {
        }

        public Task<Stream> InitializeAsync(Socket socket, Stream stream, CancellationToken cancellationToken)
        {
            return TaskEx.FromResult<Stream>(new NetworkStream(socket, true));
        }
    }
}