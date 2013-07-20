using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;
using Netro;

namespace NetroTest
{
    [TestFixture]
    public class ReverseStreamTest
    {
        private static int _port = 4010;

        [SetUp]
        public void SetUp()
        {
            _server = new TcpListener(new IPEndPoint(0, _port));
            _server.Start();

            var client = new TcpClient("localhost", _port);
            _cstream = new ReverseStream(client.GetStream());

            _port++;
        }

        private const int BufferSize = 32768;
        private ReverseStream _cstream;
        private TcpListener _server;

        private void Server(Action<ReverseStream> callback)
        {
            _server.BeginAcceptSocket(ar =>
                {
                    var sstream = new ReverseStream(_server.EndAcceptTcpClient(ar).GetStream());
                    callback(sstream);
                }, _server);
        }

        [Test]
        public void MultipleTest()
        {
            var run = true;

            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(0, 8000));
            listener.Listen(0);
            Console.WriteLine("Listening Reverse on port " + 8000);

            AsyncListener
                .AcceptAsync(listener, socket =>
                                       AsyncStream
                                           .ReadAsync(new NetworkStream(socket),
                                                      (bytes, i) =>
                                                          {
                                                              var text = Encoding.UTF8.GetString(bytes,
                                                                                                 0, i);
                                                              Console.WriteLine(text);
                                                              if (text.Contains("end")) run = false;
                                                          }));

            var c1 = new TcpClient("localhost", 8000);
            var b = Encoding.UTF8.GetBytes("Hello, world!");
            c1.GetStream().Write(b, 0, b.Length);
            b = Encoding.UTF8.GetBytes("Hello, world again!");
            c1.GetStream().Write(b, 0, b.Length);

            var c2 = new TcpClient("localhost", 8000);
            b = Encoding.UTF8.GetBytes("Hello, world again and again!");
            c2.GetStream().Write(b, 0, b.Length);
            b = Encoding.UTF8.GetBytes("Hello, world again and again and again!  -- and end");
            c2.GetStream().Write(b, 0, b.Length);

            while (run)
            {
            }
        }

        [Test]
        public void TestLargeData()
        {
            var r = new Random();
            var length = (2*BufferSize) + (BufferSize*r.Next(10));
            Console.WriteLine(length);
            var randomData = Enumerable.Range(0, length).Select(i => (byte) r.Next()).ToArray();

            var run = true;
            var pos = 0;
            Server(stream => stream.ReadAsync((id, data) =>
                {
                    Assert.AreEqual(id, 77);

                    for (var i = 0; i < data.Length; i++)
                    {
                        Assert.AreEqual(randomData[pos + i], data[i]);
                    }
                    pos += data.Length;
                    run = pos >= randomData.Length;
                }));

            _cstream.Write(77, randomData);

            while (run)
            {
            }
        }

        [Test]
        public void TestPingPong()
        {
            var run = true;

            Server(sstream => sstream.ReadAsync((id, data) =>
                {
                    Assert.AreEqual(42, id);
                    Assert.AreEqual("Hello", Encoding.UTF8.GetString(data));
                    sstream.Write(43, Encoding.UTF8.GetBytes("World!"));
                }));

            _cstream.ReadAsync((id, data) =>
                {
                    Assert.AreEqual(43, id);
                    Assert.AreEqual("World!", Encoding.UTF8.GetString(data));
                    run = false;
                });
            _cstream.Write(42, Encoding.UTF8.GetBytes("Hello"));

            while (run)
            {
            }
        }
    }
}