using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Netro;

namespace NetroTest
{
    [TestFixture]
    public class ServerClientTest
    {

        [Test]
        public void TestServerClient()
        {
            var run = true;
            const int serverPort = 4005;
            const int netroPort = 4006;

            var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(0, serverPort));
            server.Listen(int.MaxValue);

            AsyncListener.AcceptAsync(server, socket =>
                {
                    Console.WriteLine("Connected");
                    AsyncStream.ReadAsync(new NetworkStream(socket), (bytes, i) =>
                        {
                            var text = Encoding.UTF8.GetString(bytes, 0, i);
                            Console.WriteLine(text);
                            if (text.Contains("end")) run = false;
                        });
                });


            var netro = new Netro.Netro();
            netro.Listen(netroPort);
            netro.SetClient("localhost", serverPort);

            var client = new TcpClient("localhost", netroPort);
            var b = Encoding.UTF8.GetBytes("Hello! and end");
            client.GetStream().Write(b,0,b.Length);

            while (run)
            {
                
            }
        }
    }
}
