using NUnit.Framework;
using Netro;
using NetroTest.Util;

namespace NetroTest
{
    [TestFixture]
    public class SocketPipeTest : AsyncTest
    {
        [Test, Timeout(2000)]
        public void TestPipeClients()
        {
            Until((port, done) =>
                {
                    var serverA = new AsyncSocket();
                    var serverB = new AsyncSocket();

                    var socketA = new AsyncSocket();
                    var socketB = new AsyncSocket();

                    serverA.Listen(port, socket => socket.Read(text =>
                        {
                            Assert.AreEqual("Hello", text);
                            socket.Write("world!");
                        }));
                    socketA.Connect(Host, port);

                    serverB.Listen(port + 1, socket =>
                        {
                            socket.Read(text =>
                                {
                                    Assert.AreEqual("world!", text);
                                    done();
                                });
                            socket.Write("Hello");
                        });

                    socketB.Connect(Host, port + 1, () => { });

                    SocketPipe.PipeSockets(socketA, socketB);
                });
        }

        [Test, Timeout(10000)]
        public void TestPipeServerClient()
        {
            Until((port, done) =>
                {
                    var server = new AsyncSocket();

                    var socketA = new AsyncSocket();
                    var socketB = new AsyncSocket();
                    var portA = port;
                    var portB = portA + 1;

                    server.Listen(portA, socket => socket.Read(text =>
                        {
                            Assert.AreEqual("Hello", text);
                            socket.Write("world!");
                        }));

                    socketB.Listen(portB, socket =>
                        {
                            var c = new AsyncSocket();
                            c.Connect(Host, portB, () => SocketPipe.PipeSockets(socket, c));
                        });

                    socketA.Read(text =>
                        {
                            Assert.AreEqual("world!", text);
                            done();
                        });
                    socketA.Connect(Host, portA, () => socketA.Write("Hello"));
                });
        }
    }
}