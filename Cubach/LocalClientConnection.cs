using System;
using System.Collections.Generic;
using Cubach.Model;

namespace Cubach
{
    public class LocalClientConnection : IClientConnection
    {
        private readonly Queue<IMessage> serverMessageQueue;
        private readonly Queue<IMessage> clientMessageQueue;

        public event EventHandler<EventArgs> ChunksRequestReceived = (s, e) => { };

        public LocalClientConnection(Queue<IMessage> serverMessageQueue, Queue<IMessage> clientMessageQueue)
        {
            this.serverMessageQueue = serverMessageQueue;
            this.clientMessageQueue = clientMessageQueue;
        }

        public void ProcessMessages()
        {
            while (serverMessageQueue.Count > 0) {
                var message = serverMessageQueue.Dequeue();
                if (message is ChunksRequestMessage) {
                    ChunksRequestReceived(this, new EventArgs());
                }
            }
        }

        public void SendChunk(Chunk chunk) => clientMessageQueue.Enqueue(new ChunkMessage(chunk));
    }
}
