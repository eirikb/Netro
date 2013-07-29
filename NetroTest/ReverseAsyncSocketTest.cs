using System;
using NUnit.Framework;
using Netro;
using NetroTest.Util;

namespace NetroTest
{
    [TestFixture]
    public class ReverseAsyncSocketTest : AsyncTest
    {
        [Test, Timeout(2000)]
        public void TestDisconnectClient()
        {
            Until((port, done) =>
                {
                    var server = new ReverseAsyncSocket();
                    server.Listen(port, socket => socket.Disconnect());

                    var count = 2;
                    var client = new ReverseAsyncSocket();
                    client.Read((id, text) => {});
                    client.Disconnect(() => done(--count == 0));
                    client.Connect(Host, port, () => client.Disconnect(() => done(--count == 0)));
                });
        }

        [Test, Timeout(2000)]
        public void TestPingClient()
        {
            Until((port, done) =>
                {
                    var server = new ReverseAsyncSocket();
                    server.Listen(port, socket => socket.Read((id, text) =>
                        {
                            Assert.AreEqual(7, id);
                            Assert.AreEqual("Hello", text);
                            done();
                        }));

                    var client = new ReverseAsyncSocket();
                    client.Connect(Host, port, () => client.Write(7, "Hello"));
                });
        }

        [Test, Timeout(2000)]
        public void TestPingPong()
        {
            Until((port, done) =>
                {
                    var server = new ReverseAsyncSocket();
                    server.Listen(port, socket => socket.Read((id, text) =>
                        {
                            Assert.GreaterOrEqual(id, 42);
                            Assert.LessOrEqual(id, 43);
                            Assert.AreEqual("Hello", text);

                            var res = (new[] {"eirikb", "world!"})[43 - id];
                            socket.Write(id + 1, res);
                        }));

                    var client = new ReverseAsyncSocket();
                    client.Read((id, text) =>
                        {
                            Assert.GreaterOrEqual(id, 43);
                            Assert.LessOrEqual(id, 44);
                            if (id != 43) return;

                            Assert.AreEqual("world!", text);
                            client.Write(id, "Hello");
                        });

                    client.Connect(Host, port, () =>
                        {
                            client.Read((id, text) =>
                                {
                                    Assert.GreaterOrEqual(id, 43);
                                    Assert.LessOrEqual(id, 44);
                                    if (id != 44) return;

                                    Assert.AreEqual("eirikb", text);
                                    done();
                                });

                            client.Write(42, "Hello");
                        });
                });
        }

        [Test, Timeout(2000)]
        public void TestServerDisconnect()
        {
            Until((port, done) =>
                {
                    var server = new ReverseAsyncSocket();
                    server.Listen(port, socket =>
                        {
                            socket.Disconnect(done);
                            socket.Read((id, text) => {});
                        });

                    var client = new ReverseAsyncSocket();
                    client.Connect("localhost", port, client.Disconnect);
                });
        }
    }
}