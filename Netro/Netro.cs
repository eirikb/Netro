using System;
using System.Collections.Generic;

namespace Netro
{
    public class Netro
    {
        private int _activeSockets;
        private int _bytesTransferred;
        private KeyValuePair<string, int> _client;
        private bool _ready;
        private AsyncSocket _server;

        public bool Ready
        {
            get { return _ready; }
            private set
            {
                _ready = value;
                Output.Instance.Init();
            }
        }

        public int ActiveSockets
        {
            get { return _activeSockets; }
            private set
            {
                _activeSockets = value;
                Output.Instance.UpdateNumbers(this);
            }
        }

        public int BytesTransferred
        {
            get { return _bytesTransferred; }
            private set
            {
                _bytesTransferred = value;
                Output.Instance.UpdateNumbers(this);
            }
        }

        public void ServerToClient(AsyncSocket server, string host, int port, Action<AsyncSocket> callback = null)
        {
            server.Connect(socket =>
                {
                    var client = new AsyncSocket();
                    if (callback != null) client.Connect(callback);
                    client.Connect(host, port, () => SocketPipe.PipeSockets(socket, client));

                    client.Connect(s => ActiveSockets++);
                    client.Disconnect(() => ActiveSockets--);
                    socket.Connect(s => ActiveSockets++);
                    socket.Disconnect(() => ActiveSockets--);
                });
        }

        public void ReverseServerToServer(ReverseAsyncSocket reverseServer, AsyncSocket server)
        {
            var id = 0;
            reverseServer.Connect(reverseSocket =>
                {
                    ActiveSockets++;

                    server.Connect(socket =>
                        {
                            Output.Instance.Connected = true;

                            var tid = id++;
                            reverseSocket.Read(
                                (sid, buffer, index, count) =>
                                    {
                                        BytesTransferred += count;
                                        if (sid == tid) socket.Write(buffer, index, count);
                                    });
                            socket.Read((buffer, count) =>
                                {
                                    BytesTransferred += count;
                                    reverseSocket.Write(tid, buffer, 0, count);
                                });

                            ActiveSockets++;
                            socket.Disconnect(() => ActiveSockets--);
                        });
                });
        }

        public void ReverseClientToClient(ReverseAsyncSocket reverseClient, string host, int port)
        {
            var clients = new Dictionary<int, AsyncSocket>();

            reverseClient.Connect(s => ActiveSockets++);
            reverseClient.Read((id, buffer, index, count) =>
                {
                    BytesTransferred += count;

                    AsyncSocket client;
                    if (!clients.TryGetValue(id, out client))
                    {
                        client = new AsyncSocket();
                        client.Read((cbuffer, ccount) =>
                            {
                                BytesTransferred += count;
                                reverseClient.Write(id, cbuffer, 0, ccount);
                            });
                        client.Connect(host, port);

                        clients[id] = client;

                        client.Connect(socket => client.Write(buffer, index, count));
                        client.Connect(s => ActiveSockets++);
                        client.Disconnect(() => ActiveSockets--);
                    }
                    else
                    {
                        client.Write(buffer, index, count);
                    }
                });
        }

        public void SetClient(string host, int port)
        {
            _client = new KeyValuePair<string, int>(host, port);

            if (_server == null) return;

            ServerToClient(_server, host, port);
            Ready = true;

            Output.Instance.SetType(string.Format("Proxy ({0} -> {1}:{2})", _server.Port, host, port));
        }

        public void ConnectReverse(string host, int port)
        {
            Ready = true;

            var reverseClient = new ReverseAsyncSocket();
            ReverseClientToClient(reverseClient, _client.Key, _client.Value);
            Output.Instance.SetReverse(reverseClient);

            reverseClient.Connect(host, port);

            var text = string.Format("Reverse client ({0}:{1} -> {2}:{3})", host, port, _client.Key, _client.Value);
            Output.Instance.SetType(text);
        }

        public void Listen(int port)
        {
            _server = new AsyncSocket();
            _server.Listen(port);
        }

        public void ListenReverse(int port)
        {
            Ready = true;

            var reverseServer = new ReverseAsyncSocket();
            ReverseServerToServer(reverseServer, _server);
            reverseServer.Listen(port);
            Output.Instance.SetReverse(reverseServer);

            Output.Instance.SetType(string.Format("Reverse server ({0} -> {1})", port, _server.Port));
        }
    }
}