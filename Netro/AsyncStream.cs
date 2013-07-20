using System;
using System.IO;

namespace Netro
{
    public static class AsyncStream
    {
        private const int BufferSize = 32768;

        public static async void ReadAsync(Stream stream, Action<byte[], int> callback)
        {
            var buffer = new byte[BufferSize];

            var read = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (read == 0) return;

            callback(buffer, read);

            ReadAsync(stream, callback);
        }
    }
}