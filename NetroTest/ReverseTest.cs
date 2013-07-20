using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;
using Netro;

namespace NetroTest
{
    [TestFixture]
    public class ReverseTest
    {
        [Test]
        public void TestReverse()
        {
            const int testServerPort = 4000;
            const int reversePort = 4001;
            const int serverPort = 4002;
            var run = true;

            var server = new TcpListener(new IPEndPoint(0, testServerPort));
            server.Start();

            server.BeginAcceptTcpClient(ar =>
                {
                    var client = server.EndAcceptTcpClient(ar);
                    Console.WriteLine("Connected!");
                    AsyncStream.ReadAsync(client.GetStream(), (bytes, i) =>
                        {
                            Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, i));
                            run = false;
                        });
                }, server);

            var aNetro = new Netro.Netro();
            aNetro.ListenReverse(reversePort);
            aNetro.Listen(serverPort);

            var bNetro = new Netro.Netro();
            bNetro.ConnectReverse("localhost", reversePort);
            bNetro.SetClient("localhost", testServerPort);

            var testClient = new TcpClient("localhost", serverPort);
            var hello = Encoding.UTF8.GetBytes("Hello");
            Console.WriteLine("Sending data");
            testClient.GetStream().Write(hello, 0 ,hello.Length);

            while (run)
            {
            }

            aNetro.Close();
            bNetro.Close();
        }
    }
}