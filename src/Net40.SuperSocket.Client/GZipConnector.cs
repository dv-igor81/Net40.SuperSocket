﻿using System;
using System.IO.Compression;
using System.Net.Net40;
using System.Net.Sockets.Net40;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;

namespace SuperSocket.Client
{
    public class GZipConnector : ConnectorBase
    {
        private CompressionLevel _compressionLevel;

        public GZipConnector(CompressionLevel compressionLevel)
            : base()
        {
            _compressionLevel = compressionLevel;
        }

        protected override ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var stream = state.Stream;

            if (stream == null)
            {
                if (state.Socket != null)
                {
                    stream = new NetworkStream(state.Socket, true);
                }
                else
                {
                    throw new Exception("Stream from previous connector is null.");
                }
            }

            try
            {
                var pipeStream = new ReadWriteDelegateStream(
                    stream,
                    new GZipStream(stream, CompressionMode.Decompress),
                    new GZipStream(stream, CompressionMode.Compress));

                state.Stream = pipeStream;
                return new ValueTask<ConnectState>(state);
            }
            catch (Exception e)
            {
                return new ValueTask<ConnectState>(new ConnectState
                {
                    Result = false,
                    Exception = e
                });
            }
        }
    }
}
