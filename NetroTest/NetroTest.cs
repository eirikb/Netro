using System;
using NUnit.Framework;
using Netro;
using NetroTest.Util;

namespace NetroTest
{
    [TestFixture]
    public class NetroTest : AsyncTest
    {
        [Test, Timeout(2000)]
        public void ReverseClientToClient()
        {
            Until((port, done) =>
                {
                    var serverPort = port + 1;

                    var reverseServer = new ReverseAsyncSocket();
                    var server = new AsyncSocket();

                    var reverseClient = new ReverseAsyncSocket();
                    var netro = new Netro.Netro();

                    netro.ReverseClientToClient(reverseClient, Host, serverPort);

                    reverseServer.Listen(port);
                    server.Listen(serverPort);

                    reverseClient.Connect(Host, port, () => { });

                    server.Connect(socket => socket.Read(text =>
                        {
                            Assert.AreEqual("Hello", text);
                            socket.Write("world!");
                        }));

                    reverseServer.Connect(socket =>
                        {
                            socket.ReadString((id, command, text) =>
                                {
                                    Assert.AreEqual("world!", text);
                                    done();
                                });
                            socket.Write(42, "Hello");
                        });
                });
        }

        [Test, Timeout(2000)]
        public void TestReverseServerToServer()
        {
            Until((port, done) =>
                {
                    var reverseServer = new ReverseAsyncSocket();
                    var server = new AsyncSocket();

                    var reverseClient = new ReverseAsyncSocket();
                    var client = new AsyncSocket();

                    var netro = new Netro.Netro();
                    var serverPort = port + 1;

                    reverseServer.Listen(port);
                    server.Listen(serverPort);

                    netro.ReverseServerToServer(reverseServer, server);

                    reverseClient.ReadString((tid, command, text) =>
                        {
                            Assert.AreEqual("Hello", text);
                            reverseClient.Write(tid, "world!");
                        });
                    client.Read(text =>
                        {
                            Assert.AreEqual("world!", text);
                            done();
                        });

                    reverseClient.Connect(Host, port,
                                          () =>
                                              {
                                                  client.Connect(Host, serverPort, () =>
                                                      {
                                                          client.Write("Hello");
                                                      });
                                              });
                });
        }

        [Test, Timeout(2000)]
        public void TestServerToClient()
        {
            Until((port, done) =>
                {
                    var portTestServer = port;
                    var portServer = portTestServer + 1;

                    var testServer = new AsyncSocket();
                    var server = new AsyncSocket();
                    var client = new AsyncSocket();
                    var netro = new Netro.Netro();

                    server.Listen(portServer);
                    netro.ServerToClient(server, Host, portTestServer);

                    testServer.Listen(portTestServer, socket => socket.Read(text =>
                        {
                            Assert.AreEqual("Hello", text);
                            socket.Write("world!");
                        }));

                    client.Connect(Host, portServer, () =>
                        {
                            client.Read(text =>
                                {
                                    Assert.AreEqual("world!", text);
                                    done();
                                });
                            client.Write("Hello");
                        });
                });
        }

        [Test, Timeout(2000)]
        public void TestServerToClientDisconnect()
        {
            Until((port, done) =>
                {
                    var portTestServer = port;
                    var portServer = portTestServer + 1;

                    var testServer = new AsyncSocket();
                    var server = new AsyncSocket();
                    var client = new AsyncSocket();
                    var netro = new Netro.Netro();

                    server.Listen(portServer);
                    netro.ServerToClient(server, Host, portTestServer);

                    testServer.Listen(portTestServer, socket => socket.Disconnect());

                    client.Connect(Host, portServer, () =>
                        {
                            client.Read(text => { });
                            client.Disconnect(done);
                        });
                });
        }
    }
}