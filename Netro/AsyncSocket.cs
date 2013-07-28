using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Netro
{
    public class AsyncSocket
    {
        private const int BufferSize = 32768;
        private readonly List<Action<AsyncSocket>> _callbackConnect;
        private readonly List<Action> _callbackDisconnect;
        private readonly List<Func<bool>> _callbackPreconnect;
        private readonly List<Action<byte[], int>> _callbackRead;
        private readonly Socket _socket;
        protected NetworkStream Stream;
        private Boolean _reading;

        public AsyncSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _callbackPreconnect = new List<Func<bool>>();
            _callbackConnect = new List<Action<AsyncSocket>>();
            _callbackDisconnect = new List<Action>();
            _callbackRead = new List<Action<byte[], int>>();
            _reading = false;
        }

        private AsyncSocket(Socket socket) : this()
        {
            _socket = socket;
            Stream = new NetworkStream(_socket);
        }

        public virtual void Connect(Action<AsyncSocket> callback)
        {
            _callbackConnect.Add(callback);
        }

        public virtual void Preconnect(Func<bool> callback)
        {
            _callbackPreconnect.Add(callback);
        }

        public virtual void Connect(string host, int port, Action callback)
        {
            _callbackConnect.Add(socket => callback());
            Connect(host, port);
        }

        public virtual void Connect(string host, int port)
        {
            _socket.BeginConnect(host, port, ar =>
                {
                    try
                    {
                        _socket.EndConnect(ar);
                        Stream = new NetworkStream(_socket);

                        if (!_reading) BeginRead();
                        _callbackConnect.ForEach(callback => callback(this));
                    }
                    catch
                    {
                        _callbackDisconnect.ForEach(callback => callback());
                    }
                }, _socket);
        }

        private void BeginRead()
        {
            _reading = true;
            var buffer = new byte[BufferSize];
            if (!_socket.Connected)
            {
                _callbackDisconnect.ForEach(callback => callback());
                return;
            }

            Stream.BeginRead(buffer, 0, buffer.Length, ar =>
                {
                    try
                    {
                        var read = Stream.EndRead(ar);
                        if (read == 0) _socket.Disconnect(false);
                        else _callbackRead.ForEach(callback => callback(buffer, read));

                        BeginRead();
                    }
                    catch
                    {
                        _callbackDisconnect.ForEach(callback => callback());
                    }
                }, Stream);
        }

        public virtual void Listen(int port)
        {
            _socket.Bind(new IPEndPoint(0, port));
            _socket.Listen(int.MaxValue);
            BeginAccept();
        }

        public virtual void Listen(int port, Action<AsyncSocket> callback)
        {
            _callbackConnect.Add(callback);
            Listen(port);
        }

        private void BeginAccept()
        {
            _socket.BeginAccept(ar =>
                {
                    try
                    {
                        var socket = _socket.EndAccept(ar);
                        if (!_callbackPreconnect.All(callback => callback()))
                        {
                            socket.Close();
                        }
                        else
                        {
                            var asyncSocket = new AsyncSocket(socket);
                            _callbackConnect.ForEach(callback => callback(asyncSocket));
                            asyncSocket.BeginRead();
                        }
                        BeginAccept();
                    }
                    catch
                    {
                        _callbackDisconnect.ForEach(callback => callback());
                    }
                }, _socket);
        }

        public virtual void Read(Action<byte[], int> callback)
        {
            _callbackRead.Add(callback);
        }

        public virtual void Write(byte[] data)
        {
            Write(data, data.Length);
        }

        public virtual void Write(byte[] data, int count)
        {
            Write(data, 0, count);
        }

        public virtual void Write(byte[] data, int offset, int count)
        {
            Stream.BeginWrite(data, offset, count, ar => Stream.EndWrite(ar), Stream);
        }

        public void Disconnect(Action callback)
        {
            _callbackDisconnect.Add(callback);
        }

        public void Disconnect()
        {
            if (_socket != null) _socket.Disconnect(false);
        }
    }
}