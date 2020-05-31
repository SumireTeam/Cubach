using Cubach.Shared;
using Cubach.Shared.Network;
using System.Collections.Generic;

namespace Cubach.Client.Network
{
    public class LocalClientConnection : IClientConnection
    {
        public readonly Queue<ClientMessage> IncomingMessages;
        public readonly Queue<ServerMessage> OutgoingMessages;

        public long ID { get; }

        public LocalClientConnection(long id,
            Queue<ClientMessage> incomingMessages,
            Queue<ServerMessage> outgoingMessages)
        {
            ID = id;
            IncomingMessages = incomingMessages;
            OutgoingMessages = outgoingMessages;
        }

        public void SendChunk(Chunk chunk)
        {
            OutgoingMessages.Enqueue(new ChunkMessage(chunk));
        }
    }
}
