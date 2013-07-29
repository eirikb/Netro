using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Netro;
using NetroTest.Util;

namespace NetroTest
{
    [TestFixture]
    public class AsyncSocketTest : AsyncTest
    {
        [Test, Timeout(2000000)]
        public void TestClientDisconnect()
        {
            Until((port, done) =>
                {
                    var server = new AsyncSocket();
                    server.Listen(port, socket =>
                        {
                            socket.Disconnect(done);
                            socket.Read(text => { });
                        });

                    var client = new AsyncSocket();
                    client.Connect(Host, port, client.Disconnect);
                });
        }

        [Test, Timeout(2000)]
        public void TestClientPingDisconnect()
        {
            Until((port, done) =>
                {
                    var server = new AsyncSocket();
                    server.Listen(port, socket => socket.Read(text =>
                        {
                            Assert.AreEqual("Hello, world!", text);
                            socket.Disconnect();
                        }));

                    var client = new AsyncSocket();
                    client.Disconnect(done);
                    client.Connect(Host, port, () => client.Write("Hello, world!"));
                    client.Read(text => { });
                });
        }

        [Test, Timeout(5000)]
        public void TestMassiveDisconnect()
        {
            Until((port, done) =>
                {
                    const int clientCount = 100;
                    var count = 0;

                    var server = new AsyncSocket();
                    server.Listen(port, socket =>
                        {
                            count++;
                            socket.Disconnect(() => count--);
                            socket.Read(text => { });
                        });

                    var clients = Enumerable.Range(0, clientCount).Select(i =>
                        {
                            var client = new AsyncSocket();
                            client.Connect(Host, port);
                            return client;
                        }).ToList();

                    while (count < clientCount) Thread.Sleep(100);

                    Assert.AreEqual(clientCount, count);

                    clients.ForEach(client => client.Disconnect());
                    while (count > 0) Thread.Sleep(100);

                    Assert.AreEqual(0, count);
                    done();
                });
        }

        [Test, Timeout(2000)]
        public void TestPing()
        {
            Until((port, done) =>
                {
                    var server = new AsyncSocket();
                    server.Listen(port, socket => socket.Read(text =>
                        {
                            Assert.AreEqual("Hello, world!", text);
                            done();
                        }));

                    var client = new AsyncSocket();
                    client.Connect(Host, port, () => client.Write("Hello, world!"));
                });
        }

        [Test, Timeout(2000)]
        public void TestPingPong()
        {
            Until((port, done) =>
                {
                    var server = new AsyncSocket();
                    server.Listen(port, socket => socket.Read(text =>
                        {
                            Assert.AreEqual("Hello", text);
                            socket.Write("World!");
                        }));

                    var client = new AsyncSocket();
                    client.Connect(Host, port, () => client.Write("Hello"));
                    Thread.Sleep(500);
                    client.Read(text =>
                        {
                            Assert.AreEqual("World!", text);
                            done();
                        });
                });
        }

        [Test, Timeout(2000)]
        public void TestPreconnect()
        {
            Until((port, done) =>
                {
                    var count = 0;
                    var clients = 0;
                    var server = new AsyncSocket();
                    server.Listen(port);
                    server.Preconnect(() =>
                        {
                            count++;
                            return count%2 == 0;
                        });
                    server.Connect(socket =>
                        {
                            clients++;
                            if (count < 10) return;
                            Assert.AreEqual(10, count);
                            Assert.AreEqual(5, clients);
                            done();
                        });

                    for (var i = 0; i < 10; i++)
                    {
                        var client = new AsyncSocket();
                        client.Connect(Host, port);
                    }
                });
        }

        [Test, Timeout(2000)]
        public void TestServerDisconnect()
        {
            Until((port, done) =>
                {
                    var server = new AsyncSocket();
                    server.Listen(port, socket => socket.Disconnect());

                    var client = new AsyncSocket();
                    client.Disconnect(done);
                    client.Connect(Host, port);
                    client.Read(text => {});
                });
        }

        [Test, Timeout(2000)]
        public void TestServerPongReadAfterConnect()
        {
            Until((port, done) =>
                {
                    var server = new AsyncSocket();
                    server.Listen(port, socket => socket.Write("Hello"));

                    var client = new AsyncSocket();
                    client.Connect(Host, port);
                    client.Read(text =>
                        {
                            Assert.AreEqual("Hello", text);
                            done();
                        });
                });
        }
    }
}