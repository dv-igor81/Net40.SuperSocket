using System.IO;
using System.IO.Compression;
using System.Net.Sockets.Net40;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    public class GZipStreamInitializer : IConnectionStreamInitializer
    {
        public CompressionLevel CompressionLevel { get; private set; }

        public Task<Stream> InitializeAsync(Socket socket, Stream stream, CancellationToken cancellationToken)
        {
            var connectionStream = new ReadWriteDelegateStream(
                stream,
                new GZipStream(stream, CompressionMode.Decompress),
                new GZipStream(stream, CompressionMode.Compress));
                
            return TaskEx.FromResult<Stream>(connectionStream);
        }

        public void Setup(ListenOptions listenOptions)
        {
            CompressionLevel = CompressionLevel.Optimal;
        }
    }
}