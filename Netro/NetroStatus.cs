using System;

namespace Netro
{
    public class NetroStatus
    {
        public enum Type
        {
            None,
            ReverseServer,
            ReverseClient,
            Proxy
        }

        private readonly Netro _netro;

        public NetroStatus(Netro netro)
        {
            _netro = netro;
        }

        public Type CurrentType
        {
            get
            {
                if (_netro.ReverseServer != null) return Type.ReverseServer;
                if (_netro.ReverseClient != null) return Type.ReverseClient;
                return _netro.Server != null ? Type.Proxy : Type.None;
            }
        }

        public string From
        {
            get
            {
                switch (CurrentType)
                {
                    case Type.ReverseServer:
                        return "" + _netro.ReverseServer.Port;
                    case Type.ReverseClient:
                        return "" + _netro.ReverseClient.Host + ":" + _netro.ReverseClient.Port;
                    case Type.Proxy:
                        return "" + _netro.Server.Port;
                    default:
                        return "";
                }
            }
        }

        public string To
        {
            get
            {
                switch (CurrentType)
                {
                    case Type.ReverseServer:
                        return "" + _netro.Server.Port;
                    case Type.ReverseClient:
                    case Type.Proxy:
                        return _netro.Client.Key + ":" + _netro.Client.Value;
                    default:
                        return "";
                }
            }
        }

        public void OnConnect(Action<bool> callback)
        {
            var reverse = _netro.ReverseServer ?? _netro.ReverseClient;
            var socket = _netro.Server;

            if (reverse != null)
            {
                reverse.Connect(s => callback(true));
                reverse.Disconnect(() => callback(false));
                if (reverse.Connected) callback(true);

                return;
            }

            if (socket == null) return;

            socket.Connect(s => callback(true));
            socket.Disconnect(() => callback(false));
            if (socket.Connected) callback(true);
        }
    }
}