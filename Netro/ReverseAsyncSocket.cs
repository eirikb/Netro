using System;
using System.Collections.Generic;

namespace Netro
{
    public enum Command : byte
    {
        Connect = 1,
        Data = 2,
        Disconnect = 3
    }

    public class ReverseAsyncSocket
    {
        private readonly List<Action<ReverseAsyncSocket>> _callbackConnect;
        private readonly List<Action<int, Command, byte[]>> _callbackRead;
        private readonly AsyncSocket _socket;
        private Command _command;
        private int _currentId;
        private bool _reading;
        private int _waitingFor;

        public ReverseAsyncSocket()
        {
            _socket = new AsyncSocket();
            _callbackRead = new List<Action<int, Command, byte[]>>();
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


        public void Read(Action<int, Command, byte[]> callback)
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
                            _command = (Command) buffer[pos + 8];
                            pos += 9;
                        }

                        var count = read - pos;
                        if (count > _waitingFor) count = _waitingFor;

                        var data = new byte[count];
                        Array.Copy(buffer, pos, data, 0, count);

                        _callbackRead.ForEach(cb => cb(_currentId, _command, data));

                        pos += count;
                        _waitingFor -= count;
                    }
                });
        }

        public void Write(int id, Command command, byte[] buffer, int index, int count)
        {
            var newBuffer = new byte[count + 9];
            BitConverter.GetBytes(id).CopyTo(newBuffer, 0);
            BitConverter.GetBytes(count).CopyTo(newBuffer, 4);
            newBuffer[8] = (byte) command;
            Array.Copy(buffer, index, newBuffer, 9, count);
            _socket.Write(newBuffer, 0, newBuffer.Length);
        }
    }
}