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
    }
}