using System;
using System.Collections.Generic;
using Cubach.Shared;
using Cubach.Shared.Network;
using Lidgren.Network;

namespace Cubach.Server.Network
{
    public class ClientManager : IClientManager
    {
        private readonly NetServer server;

        private readonly Dictionary<long, ClientConnection> connections =
            new Dictionary<long, ClientConnection>();

        public event EventHandler<ErrorEventArgs> Error = (s, e) => { };
        public event EventHandler<ClientConnectionEventArgs> Connected = (s, e) => { };
        public event EventHandler<ClientConnectionEventArgs> Disconnected = (s, e) => { };
        public event EventHandler<ClientConnectionEventArgs> ChunksRequestReceived = (s, e) => { };

        public ClientManager(int port)
        {
            var config = new NetPeerConfiguration("cubach") {Port = port};

            server = new NetServer(config);
            server.Start();
        }

        public void Run() { }

        private void ProcessStatusMessage(NetIncomingMessage msg)
        {
            var status = (NetConnectionStatus) msg.ReadByte();
            switch (status) {
                case NetConnectionStatus.Connected: {
                    var id = msg.SenderConnection.RemoteUniqueIdentifier;
                    var connection = new ClientConnection(msg.SenderConnection);
                    connections.Add(id, connection);
                    Connected(this, new ClientConnectionEventArgs(connection));
                    break;
                }

                case NetConnectionStatus.Disconnected: {
                    var id = msg.SenderConnection.RemoteUniqueIdentifier;
                    if (connections.TryGetValue(id, out var connection)) {
                        Disconnected(this, new ClientConnectionEventArgs(connection));
                        connections.Remove(id);
                    }

                    break;
                }
            }
        }

        private void ProcessDataMessage(NetIncomingMessage msg)
        {
            var type = (ClientMessageType) msg.ReadByte();
            switch (type) {
                case ClientMessageType.ChunksRequest: {
                    var id = msg.SenderConnection.RemoteUniqueIdentifier;
                    if (connections.TryGetValue(id, out var connection)) {
                        ChunksRequestReceived(this, new ClientConnectionEventArgs(connection));
                    }

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
            while ((msg = server.ReadMessage()) != null) {
                switch (msg.MessageType) {
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

                server.Recycle(msg);
            }
        }

        public void SendChunk(Chunk chunk)
        {
            var message = server.CreateMessage();
            message.Write((byte) ServerMessageType.Chunk);
            message.WriteChunk(chunk);

            server.SendToAll(message, NetDeliveryMethod.ReliableUnordered);
        }
    }
}
