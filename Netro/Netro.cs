using System;
using System.Collections.Generic;

namespace Netro
{
    public class Netro
    {
        internal KeyValuePair<string, int> Client;
        internal ReverseAsyncSocket ReverseClient;
        internal ReverseAsyncSocket ReverseServer;
        internal AsyncSocket Server;

        public void ServerToClient(AsyncSocket server, string host, int port, Action<AsyncSocket> callback = null)
        {
            server.Connect(socket =>
                {
                    var client = new AsyncSocket();
                    if (callback != null) client.Connect(callback);
                    client.Connect(host, port, () => SocketPipe.PipeSockets(socket, client));
                });
        }

        public void ReverseServerToServer(ReverseAsyncSocket reverseServer, AsyncSocket server)
        {
            var id = 0;
            reverseServer.Connect(reverseSocket => server.Connect(socket =>
                {
                    var tid = id++;
                    reverseSocket.Read((sid, command, buffer) => { if (sid == tid) socket.Write(buffer); });
                    socket.Read((buffer, count) =>
                                reverseSocket.Write(tid, Command.Data, buffer, 0, count));
                }));
        }

        public void ReverseClientToClient(ReverseAsyncSocket reverseClient, string host, int port)
        {
            var clients = new Dictionary<int, AsyncSocket>();

            reverseClient.Read((id, command, buffer) =>
                {
                    AsyncSocket client;
                    if (!clients.TryGetValue(id, out client))
                    {
                        client = new AsyncSocket();
                        client.Read((cbuffer, ccount) => reverseClient.Write(id, Command.Data, cbuffer, 0, ccount));
                        client.Connect(host, port);

                        clients[id] = client;

                        client.Connect(socket => client.Write(buffer));
                    }
                    else
                    {
                        client.Write(buffer);
                    }
                });
        }

        public void SetClient(string host, int port)
        {
            Client = new KeyValuePair<string, int>(host, port);

            if (Server == null) return;

            ServerToClient(Server, host, port);
        }

        public void ConnectReverse(string host, int port)
        {
            ReverseClient = new ReverseAsyncSocket();
            ReverseClientToClient(ReverseClient, Client.Key, Client.Value);
            ReverseClient.Connect(host, port);
        }

        public void Listen(int port)
        {
            Server = new AsyncSocket();
            Server.Listen(port);
        }

        public void ListenReverse(int port)
        {
            ReverseServer = new ReverseAsyncSocket();
            ReverseServerToServer(ReverseServer, Server);
            ReverseServer.Listen(port);
        }
    }
}