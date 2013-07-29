using System;
using System.Collections.Generic;

namespace Netro
{
    public class ReverseAsyncSocket
    {
        private readonly List<Action<ReverseAsyncSocket>> _callbackConnect;
        private readonly List<Action<int, byte[], int, int>> _callbackRead;
        private readonly AsyncSocket _socket;
        private int _currentId;
        private bool _reading;
        private int _waitingFor;

        public ReverseAsyncSocket()
        {
            _socket = new AsyncSocket();
            _callbackRead = new List<Action<int, byte[], int, int>>();
            _callbackConnect = new List<Action<ReverseAsyncSocket>>();
        }

        private ReverseAsyncSocket(AsyncSocket socket) : this()
        {
            _socket = socket;
        }

        public bool Connected
        {
            get { return _socket.Connected; }
        }

        public int Port
        {
            get { return _socket.Port; }
        }

        public string Host
        {
            get { return _socket.Host; }
        }

        public void Listen(int port)
        {
            _socket.Listen(port);
        }

        public void Listen(int port, Action<ReverseAsyncSocket> callback)
        {
            Connect(callback);
            _socket.Listen(port);
        }

        public void Connect(Action<ReverseAsyncSocket> callback)
        {
            _callbackConnect.Add(callback);
            if (_callbackConnect.Count > 1) return;

            _socket.Connect(socket =>
                {
                    var reverseSocket = new ReverseAsyncSocket(socket);
                    _callbackConnect.ForEach(cb => cb(reverseSocket));
                });
        }

        public void Connect(string host, int port)
        {
            _socket.Connect(host, port);
        }

        public void Connect(string host, int port, Action callback)
        {
            _socket.Connect(host, port, callback);
        }

        public void Disconnect(Action callback)
        {
            _socket.Disconnect(callback);
        }

        public void Disconnect()
        {
            _socket.Disconnect();
        }

        public void Read(Action<int, byte[], int, int> callback)
        {
            _callbackRead.Add(callback);

            if (_reading) return;
            _reading = true;

            _socket.Read((buffer, read) =>
                {
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

                        _callbackRead.ForEach(cb => cb(_currentId, buffer, pos, next));

                        pos += next;
                        _waitingFor -= next;
                    }
                });
        }

        public void Write(int id, byte[] buffer, int index, int count)
        {
            var newBuffer = new byte[count + 8];
            BitConverter.GetBytes(id).CopyTo(newBuffer, 0);
            BitConverter.GetBytes(count).CopyTo(newBuffer, 4);
            Array.Copy(buffer, index, newBuffer, 8, count);
            _socket.Write(newBuffer, 0, newBuffer.Length);
        }
    }
}