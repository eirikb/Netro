using System.Linq;
using NUnit.Framework;
using Netro;
using NetroTest.Util;

namespace NetroTest
{
    [TestFixture]
    public class AsyncSocketTest : AsyncTest
    {
        [Test, Timeout(2000)]
        public void TestClientDisconnect()
        {
            Until((port, done) =>
                {
                    var server = new AsyncSocket();
                    server.Listen(port, socket => socket.Disconnect(done));

                    var client = new AsyncSocket();
                    client.Connect("localhost", port, client.Disconnect);
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
                    client.Connect("localhost", port, () => client.Write("Hello, world!"));
                });
        }

        [Test, Timeout(10000)]
        public void TestMassiveDisconnect()
        {
            Until((port, done) =>
                {
                    const int clientCount = 100;
                    var count = 0;

                    var server = new AsyncSocket();
                    server.Listen(port, socket => socket.Disconnect(() => count--));

                    var clients = Enumerable.Range(0, clientCount).Select(i =>
                        {
                            var client = new AsyncSocket();
                            client.Connect("localhost", port, () => count++);
                            return client;
                        }).ToList();

                    while (count < clientCount)
                    {
                    }
                    Assert.AreEqual(clientCount, count);

                    clients.ForEach(client => client.Disconnect());
                    while (count > 0)
                    {
                    }
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
                    client.Connect("localhost", port, () => client.Write("Hello, world!"));
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
                    client.Read(text =>
                        {
                            Assert.AreEqual("World!", text);
                            done();
                        });
                    client.Connect("localhost", port, () => client.Write("Hello"));
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
                    client.Connect("localhost", port);
                });
        }
    }
}