using System.Buffers;
using System.IO;
using System.Text;

namespace AppleTools
{
    static class StreamExtensions
    {
        public static string ReadUtf8FixedWidthString(this Stream stream, uint byteLength)
        {
            if (byteLength == 0) return string.Empty;

            var buffer = ArrayPool<byte>.Shared.Rent((int)byteLength);
            try
            {
                var dataLength = stream.Read(buffer, 0, (int)byteLength);
                if (dataLength < 0) throw new EndOfStreamException("Unexpected end of stream while trying to read data");

                var byteReadLength = (uint)dataLength;
                if (byteReadLength != byteLength) throw new EndOfStreamException($"Not enough data read {byteReadLength} bytes while expecting to read {byteLength} bytes");

                int nullIndex = buffer.AsSpan(0, dataLength).IndexOf((byte)0);

                var text = Encoding.UTF8.GetString(buffer, 0, nullIndex >= 0 ? nullIndex : dataLength);
                return text;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}