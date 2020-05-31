using Cubach.Shared;
using Cubach.Shared.Network;
using Lidgren.Network;

namespace Cubach.Server.Network
{
    public class ClientConnection : IClientConnection
    {
        private readonly NetConnection connection;

        public long ID => connection.RemoteUniqueIdentifier;

        public ClientConnection(NetConnection connection) => this.connection = connection;

        public void SendChunk(Chunk chunk)
        {
            var message = connection.Peer.CreateMessage();
            message.Write((byte) ServerMessageType.Chunk);
            message.WriteChunk(chunk);

            connection.SendMessage(message, NetDeliveryMethod.ReliableUnordered, 0);
        }

        public void Disconnect() => connection.Disconnect("");
    }
}
