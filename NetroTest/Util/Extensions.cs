using System;
using System.Text;
using Netro;

namespace NetroTest.Util
{
    public static class AsyncSocketExtensions
    {
        public static void Write(this AsyncSocket socket, string text)
        {
            var data = Encoding.UTF8.GetBytes(text);
            socket.Write(data, 0, data.Length);
        }

        public static void Read(this AsyncSocket socket, Action<string> callback)
        {
            socket.Read((buffer, read) => callback(Encoding.UTF8.GetString(buffer, 0, read)));
        }

        public static void Write(this ReverseAsyncSocket socket, int id, string text)
        {
            var data = Encoding.UTF8.GetBytes(text);
            socket.Write(id, Command.Data, data, 0, data.Length);
        }

        public static void ReadString(this ReverseAsyncSocket socket, Action<int, Command, string> callback)
        {
            socket.Read((id, command, data) => callback(id, command, Encoding.UTF8.GetString(data)));
        }
    }
}