using Cubach.Model;
using Lidgren.Network;

namespace Cubach.Network.Remote
{
    public class RemoteClientConnection : IClientConnection
    {
        private readonly NetConnection connection;

        public long ID => connection.RemoteUniqueIdentifier;

        public RemoteClientConnection(NetConnection connection) => this.connection = connection;

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
