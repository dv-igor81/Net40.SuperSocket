using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;

namespace SuperSocket.Connection
{
    public class ReadWriteDelegateStream : Stream
    {
        public Stream BaseStream { get; }
        Stream readStream = null;
        Stream writeStream = null;
        public ReadWriteDelegateStream(Stream stream, Stream readStream, Stream writeStream)
        {
            this.readStream = readStream;
            this.writeStream = writeStream;
            this.BaseStream = stream;
        }
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
            writeStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer, offset, count);    
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();

        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            writeStream.Write(buffer, offset, count);
        }
        public void Write(ReadOnlySpan<byte> buffer)
        {
            writeStream.Write(buffer);
        }
        public int Read(Span<byte> buffer)
        {
            return readStream.Read(buffer);
        }
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return readStream.BeginRead(buffer, offset, count, callback, state);
        }
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return writeStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            BaseStream.Close();
        }

        public void CopyTo(Stream destination, int bufferSize)
        {
            readStream.CopyTo(destination, bufferSize);
        }

        public Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return readStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }


        public async ValueTask DisposeAsync()
        {
            await readStream.DisposeAsync();
            await writeStream.DisposeAsync();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return readStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            writeStream.EndWrite(asyncResult);
        }

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            return writeStream.FlushAsync(cancellationToken);
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return readStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return readStream.ReadAsync(buffer, cancellationToken);
        }

        public override int ReadByte()
        {
            return readStream.ReadByte();
        }

        public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return writeStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return writeStream.WriteAsync(buffer, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            writeStream.WriteByte(value);
        }

        public override bool CanTimeout => BaseStream.CanTimeout;

        public override int ReadTimeout { get => BaseStream.ReadTimeout; set => BaseStream.ReadTimeout = value; }
        public override int WriteTimeout { get => BaseStream.WriteTimeout; set => BaseStream.WriteTimeout = value; }
    }

}