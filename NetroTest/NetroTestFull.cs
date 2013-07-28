using NUnit.Framework;
using Netro;
using NetroTest.Util;

namespace NetroTest
{
    [TestFixture]
    public class NetroTestFull : AsyncTest
    {
        [Test, Timeout(2000)]
        public void FullTest()
        {
            Until((port, done) =>
                {
                    var serverPortA = port + 1;
                    var serverPortB = port + 2;

                    var serverA = new AsyncSocket();
                    var serverB = new AsyncSocket();
                    var clientA = new AsyncSocket();

                    var reverseServer = new ReverseAsyncSocket();
                    var reverseClient = new ReverseAsyncSocket();

                    var netro = new Netro.Netro();

                    serverA.Listen(serverPortA);
                    serverB.Listen(serverPortB);
                    reverseServer.Listen(port);

                    netro.ReverseServerToServer(reverseServer, serverA);
                    netro.ReverseClientToClient(reverseClient, Host, serverPortB);

                    reverseServer.Connect(s =>
                        {
                            serverB.Connect(socket => socket.Read(text =>
                                {
                                    Assert.AreEqual("Hello", text);
                                    socket.Write("world!");
                                }));

                            clientA.Connect(Host, serverPortA, () =>
                                {
                                    clientA.Write("Hello");
                                    clientA.Read(text =>
                                        {
                                            Assert.AreEqual("world!", text);
                                            done();
                                        });
                                });
                        });

                    reverseClient.Connect(Host, port, () => { });
                });
        }
    }
}