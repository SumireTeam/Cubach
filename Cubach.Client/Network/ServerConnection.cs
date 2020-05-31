using System;
using Cubach.Shared;
using Cubach.Shared.Network;
using Lidgren.Network;

namespace Cubach.Client.Network
{
    public class ServerConnection : IServerConnection
    {
        private readonly NetClient client;
        private readonly string host;
        private readonly int port;

        public event EventHandler<ErrorEventArgs> Error = (s, e) => { };
        public event EventHandler Connected = (s, e) => { };
        public event EventHandler Disconnected = (s, e) => { };
        public event EventHandler<ChunkEventArgs> ChunkReceived = (s, e) => { };

        public ServerConnection(string host, int port)
        {
            this.host = host;
            this.port = port;

            var config = new NetPeerConfiguration("cubach");

            client = new NetClient(config);
            client.Start();
        }

        public void Run()
        {
            client.Connect(host, port);
        }

        private void ProcessStatusMessage(NetIncomingMessage msg)
        {
            var status = (NetConnectionStatus)msg.ReadByte();
            switch (status)
            {
                case NetConnectionStatus.Connected:
                    Connected(this, EventArgs.Empty);
                    break;

                case NetConnectionStatus.Disconnected:
                    Disconnected(this, EventArgs.Empty);
                    break;
            }
        }

        private void ProcessDataMessage(NetIncomingMessage msg)
        {
            var type = (ServerMessageType)msg.ReadByte();
            switch (type)
            {
                case ServerMessageType.Chunk:
                    {
                        var chunk = msg.ReadChunk();
                        ChunkReceived(this, new ChunkEventArgs(chunk));
                        break;
                    }
            }
        }

        private void ProcessErrorMessage(NetIncomingMessage msg)
        {
            var message = msg.ReadString();
            Error(this, new ErrorEventArgs(message));
        }

        private void ProcessUnhandledMessage(NetIncomingMessage msg)
        {
            var message = $"Unhandled type: {msg.MessageType}";
            Error(this, new ErrorEventArgs(message));
        }

        public void ProcessMessages()
        {
            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        ProcessStatusMessage(msg);
                        break;

                    case NetIncomingMessageType.Data:
                        ProcessDataMessage(msg);
                        break;

                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        ProcessErrorMessage(msg);
                        break;

                    default:
                        ProcessUnhandledMessage(msg);
                        break;
                }

                client.Recycle(msg);
            }
        }

        public void RequestChunks()
        {
            var message = client.CreateMessage();
            message.Write((byte)ClientMessageType.ChunksRequest);

            client.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }
    }
}
