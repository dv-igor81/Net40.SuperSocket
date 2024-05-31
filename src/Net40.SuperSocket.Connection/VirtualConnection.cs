﻿using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    public abstract class VirtualConnection : PipeConnection, IVirtualConnection
    {
        public VirtualConnection(ConnectionOptions options)
            : base(options)
        {
 
        }

        internal override Task FillPipeAsync(PipeWriter writer, ISupplyController supplyController, CancellationToken cancellationToken)
        {
            return TaskExEx.CompletedTask;
        }

        public async ValueTask<FlushResult> WritePipeDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            return await Input.Writer.WriteAsync(memory, cancellationToken).ConfigureAwait(false);
        }
    }
}