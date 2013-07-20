using System;
using System.IO;

namespace Netro
{
    public class ReverseStream
    {
        private readonly Stream _stream;
        private int _currentId;
        private int _waitingFor;

        public ReverseStream(Stream stream)
        {
            _stream = stream;
        }

        public void ReadAsync(Action<int, byte[]> callback)
        {
            AsyncStream.ReadAsync(_stream, (buffer, read) =>
                {
                    if (read == 0) return;

                    var pos = 0;
                    while (pos < read)
                    {
                        if (_waitingFor == 0)
                        {
                            _currentId = BitConverter.ToInt32(buffer, pos);
                            _waitingFor = BitConverter.ToInt32(buffer, pos + 4);
                            pos += 8;
                        }

                        var next = read - pos;
                        if (next > _waitingFor) next = _waitingFor;

                        Callback(buffer, pos, pos + next, callback);

                        pos += next;
                        _waitingFor -= next;
                    }
                });
        }

        private void Callback(byte[] buffer, int start, int stop, Action<int, byte[]> callback)
        {
            var data = new byte[stop - start];
            Buffer.BlockCopy(buffer, start, data, 0, data.Length);
            callback(_currentId, data);
        }

        public void Write(int id, byte[] buffer)
        {
            Write(id, buffer, buffer.Length);
        }

        public void Write(int id, byte[] buffer, int size)
        {
            var newBuffer = new byte[size + 8];
            BitConverter.GetBytes(id).CopyTo(newBuffer, 0);
            BitConverter.GetBytes(size).CopyTo(newBuffer, 4);
            Array.Copy(buffer, 0, newBuffer, 8, size);
            _stream.Write(newBuffer, 0, newBuffer.Length);
        }

        public void Close()
        {
            _stream.Close();
        }
    }
}