using System;
using System.Net.Sockets;

namespace Netro
{
    public class AsyncListener
    {
        public static void AcceptAsync(Socket listener, Action<Socket> callback)
        {
            listener.BeginAccept(ar =>
                {
                    try
                    {
                        var socket = listener.EndAccept(ar);
                        callback(socket);
                        AcceptAsync(listener, callback);
                    }
                    catch
                    {
                    }
                }, listener);
        }
    }
}