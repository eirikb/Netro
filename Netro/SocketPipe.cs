namespace Netro
{
    public static class SocketPipe
    {
        public static void PipeSockets(AsyncSocket socketA, AsyncSocket socketB)
        {
            socketA.Read(socketB.Write);
            socketB.Read(socketA.Write);

            socketA.Disconnect(socketB.Disconnect);
            socketB.Disconnect(socketA.Disconnect);
        }

        public static void PipeReverseSocket(int id, ReverseAsyncSocket reverseSocket, AsyncSocket socket)
        {
            reverseSocket.Read((tid, buffer, index, count) => { if (tid == id) socket.Write(buffer, index, count); });
            socket.Read((buffer, count) => reverseSocket.Write(id, buffer, 0, count));
        }
    }
}