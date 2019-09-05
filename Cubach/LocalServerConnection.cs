using System;
using System.Collections.Generic;
using Cubach.Model;

namespace Cubach
{
    public class LocalServerConnection : IServerConnection
    {
        private readonly Queue<IMessage> clientMessageQueue;
        private readonly Queue<IMessage> serverMessageQueue;

        public event EventHandler<ChunkEventArgs> ChunkReceived = (s, e) => { };

        public LocalServerConnection(Queue<IMessage> clientMessageQueue, Queue<IMessage> serverMessageQueue)
        {
            this.clientMessageQueue = clientMessageQueue;
            this.serverMessageQueue = serverMessageQueue;
        }

        public void ProcessMessages()
        {
            while (clientMessageQueue.Count > 0) {
                var message = clientMessageQueue.Dequeue();
                if (message is ChunkMessage chunkMessage) {
                    ChunkReceived(this, new ChunkEventArgs(chunkMessage.Chunk));
                }
            }
        }

        public void RequestChunks() => serverMessageQueue.Enqueue(new ChunksRequestMessage());
    }
}
