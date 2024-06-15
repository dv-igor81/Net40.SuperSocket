using System;
using System.Net.Net40;
using System.Net.Sockets.Net40;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public class SocketConnector : ConnectorBase
    {
        public IPEndPoint LocalEndPoint { get; private set; }

        public SocketConnector()
            : base()
        {

        }

        public SocketConnector(IPEndPoint localEndPoint)
            : base()
        {
            LocalEndPoint = localEndPoint;
        }

        protected override async ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var addressFamily = remoteEndPoint.AddressFamily;

            if (addressFamily == AddressFamily.Unspecified)
                addressFamily = AddressFamily.InterNetworkV6;

            var socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                var localEndPoint = LocalEndPoint;

                if (localEndPoint != null)
                {
                    socket.ExclusiveAddressUse = false;
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                    socket.Bind(localEndPoint);
                }

                Task connectTask = socket.ConnectAsync(remoteEndPoint);

                var tcs = new TaskCompletionSource<bool>();
                cancellationToken.Register(() => tcs.SetResult(false));

                await TaskEx.WhenAny(new[] { connectTask, tcs.Task });

                if (!socket.Connected)
                {
                    socket.Close();

                    return new ConnectState
                    {
                        Result = false,
                    };
                }
            }
            catch (Exception e)
            {
                return new ConnectState
                {
                    Result = false,
                    Exception = e
                };
            }

            return new ConnectState
            {
                Result = true,
                Socket = socket
            };            
        }
    }
}