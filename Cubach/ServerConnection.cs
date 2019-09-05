using System;
using System.Collections.Generic;
using Cubach.Model;

namespace Cubach
{
    public interface IMessage { }

    public class ChunkMessage : IMessage
    {
        public readonly Chunk Chunk;

        public ChunkMessage(Chunk chunk)
        {
            Chunk = chunk;
        }
    }

    public interface IServerConnection
    {
        event EventHandler<ChunkEventArgs> ChunkReceived;

        void ProcessMessages();
        void RequestChunks();
    }

    public class LocalServerConnection : IServerConnection
    {
        private readonly Server server;
        private readonly Queue<IMessage> messages = new Queue<IMessage>();

        public event EventHandler<ChunkEventArgs> ChunkReceived = (s, e) => { };

        public LocalServerConnection(Server server)
        {
            this.server = server;
            this.server.World.ChunkUpdated += (s, e) => { messages.Enqueue(new ChunkMessage(e.Chunk)); };
        }

        public void ProcessMessages()
        {
            while (messages.Count > 0) {
                var message = messages.Dequeue();
                if (message is ChunkMessage chunkMessage) {
                    ChunkReceived(this, new ChunkEventArgs(chunkMessage.Chunk));
                }
            }
        }

        public void RequestChunks()
        {
            var world = server.World;
            for (var i = 0; i < World.Length; ++i) {
                for (var j = 0; j < World.Length; ++j) {
                    for (var k = 0; k < World.Length; ++k) {
                        var chunk = world.GetChunk(i, j, k);
                        messages.Enqueue(new ChunkMessage(chunk));
                    }
                }
            }
        }
    }

    public class RemoteServerConnection : IServerConnection
    {
        public event EventHandler<ChunkEventArgs> ChunkReceived = (s, e) => { };

        public void ProcessMessages()
        {
            throw new NotImplementedException();
        }

        public void RequestChunks()
        {
            throw new NotImplementedException();
        }
    }
}
