using System;
using System.Buffers;
using System.Text;
using System.Buffers.Text;

namespace SuperSocket.ProtoBase
{

    public static class Extensions
    {
        public static string ReadString(ref this SequenceReader<byte> reader, long length = 0)
        {
            return ReadString(ref reader, Encoding.UTF8, length);
        }
        
        public static string ReadString(ref this SequenceReader<byte> reader, Encoding encoding, long length = 0)
        {
            if (length == 0)
                length = reader.Remaining;

            var seq = reader.Sequence.Slice(reader.Consumed, length);
            
            try
            {                
                return seq.GetString(encoding);
            }
            finally
            {
                reader.Advance(length);
            }
        }
        public static bool TryReadBigEndian(ref this SequenceReader<byte> reader, out ushort value)
        {
            value = 0;

            if (reader.Remaining < 2)
                return false;

            if (!reader.TryRead(out byte h))
                return false;

            if (!reader.TryRead(out byte l))
                return false;

            value = (ushort)(h * 256 + l);
            return true;
        }

        public static bool TryReadBigEndian(ref this SequenceReader<byte> reader, out uint value)
        {
            value = 0;

            if (reader.Remaining < 4)
                return false;
            
            var v = 0;
            var unit = (int)Math.Pow(256, 3);

            for (var i = 0; i < 4; i++)
            {
                if (!reader.TryRead(out byte b))
                    return false;

                v += unit * b;
                unit = unit / 256;
            }

            value = (uint)v;
            return true;
        }

        public static bool TryReadBigEndian(ref this SequenceReader<byte> reader, out ulong value)
        {
            value = 0;

            if (reader.Remaining < 8)
                return false;
            
            var v = 0L;
            var unit = (long)Math.Pow(256, 7);

            for (var i = 0; i < 8; i++)
            {
                if (!reader.TryRead(out byte b))
                    return false;
                
                v += unit * b;
                unit = unit / 256;
            }

            value = (ulong)v;
            return true;
        }
        
        public static string GetString(this ReadOnlySequence<byte> buffer, Encoding encoding)
        {
            byte[] arr;
            if (buffer.IsSingleSegment)
            {
                arr = buffer.First.Span.ToArray();
                return arr.Length == 0 ? string.Empty : encoding.GetString(arr);
            }
            var sb = new StringBuilder();
            foreach (ReadOnlyMemory<byte> piece in buffer)
            {
                arr = piece.Span.ToArray();
                if (arr.Length > 0)
                {
                    sb.Append(encoding.GetString(arr));
                }
            }
            return sb.ToString();
        }

        public static int Write(this IBufferWriter<byte> writer, string text, Encoding encoding)
        {
            return writer.Write(text.ToCharArray(), encoding);
        }

        public static int Write(this IBufferWriter<byte> writer, ReadOnlySpan<char> text, Encoding encoding)
        {
            var encoder = encoding.GetEncoder();
            var completed = false;
            var totalBytes = 0;

            var minSpanSizeHint = encoding.GetMaxByteCount(1);

            while (!completed)
            {
                var span = writer.GetSpan(minSpanSizeHint);

                encoder.Convert(text, span, false, out int charsUsed, out int bytesUsed, out completed);
                
                if (charsUsed > 0)
                    text = text.Slice(charsUsed);

                totalBytes += bytesUsed;
                writer.Advance(bytesUsed);
            }

            return totalBytes;
        }
        
        /*public static int Write(this IBufferWriter<byte> writer, ReadOnlySpan<char> text, Encoding encoding)
        {
            Encoder encoder = encoding.GetEncoder();
            bool completed = false;
            int num = 0;
            int maxByteCount = encoding.GetMaxByteCount(1);
            while (!completed)
            {
                Span<byte> span = writer.GetSpan(maxByteCount);
                encoder.Convert(text, span, flush: false, out var charsUsed, out var bytesUsed, out completed);
                if (charsUsed > 0)
                {
                    text = text.Slice(charsUsed);
                }
                num += bytesUsed;
                writer.Advance(bytesUsed);
            }
            return num;
        }*/
    }
}