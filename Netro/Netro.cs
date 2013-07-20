using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Netro
{
    public class Netro
    {
        private KeyValuePair<string, int> _client;
        private Socket _reverseServer;
        private ReverseStream _reverseStream;
        private Socket _server;
        private int _threadId;

        public Netro()
        {
            _threadId = 0;
        }

        public void SetClient(string host, int port)
        {
            _client = new KeyValuePair<string, int>(host, port);
        }

        public void Listen(int listenPort)
        {
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _server.Bind(new IPEndPoint(0, listenPort));
            _server.Listen(int.MaxValue);


            AsyncListener.AcceptAsync(_server, socket =>
                {
                    if (_reverseStream != null) ListenToReverse(socket);
                    else ListenToClient(socket);
                });
        }

        private void ListenToReverse(Socket socket)
        {
            var serverStream = new NetworkStream(socket);

            var id = ++_threadId;
            _reverseStream.ReadAsync((i, bytes) => serverStream.Write(bytes, 0, bytes.Length));
            AsyncStream.ReadAsync(serverStream, (bytes, i) => _reverseStream.Write(id, bytes, i));
        }

        private void ListenToClient(Socket socket)
        {
            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(_client.Key, _client.Value);
            var clientStream = new NetworkStream(client);

            var serverStream = new NetworkStream(socket);
            AsyncStream.ReadAsync(clientStream, (bytes, i) => serverStream.Write(bytes, 0, i));
            AsyncStream.ReadAsync(serverStream, (bytes, i) => clientStream.Write(bytes, 0, i));
        }

        public void ListenReverse(int listenPort)
        {
            _reverseServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _reverseServer.Bind(new IPEndPoint(0, listenPort));
            _reverseServer.Listen(int.MaxValue);

            AsyncListener.AcceptAsync(_reverseServer,
                                      socket => _reverseStream = new ReverseStream(new NetworkStream(socket)));
        }

        public void ConnectReverse(string host, int port)
        {
            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(host, port);
            var clientStream = new ReverseStream(new NetworkStream(client));

            var streams = new Dictionary<int, NetworkStream>();

            clientStream.ReadAsync((id, data) =>
                {
                    NetworkStream stream;
                    if (!streams.TryGetValue(id, out stream))
                    {
                        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(_client.Key, _client.Value);
                        stream = new NetworkStream(socket);
                        streams[id] = stream;
                        AsyncStream.ReadAsync(stream, (bytes, size) => clientStream.Write(id, bytes, size));
                    }

                    stream.Write(data, 0, data.Length);
                });
        }

        public void Close()
        {
            if (_server != null) _server.Close();
            if (_reverseServer != null) _reverseServer.Close();
        }
    }
}