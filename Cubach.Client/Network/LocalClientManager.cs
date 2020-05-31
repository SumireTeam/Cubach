using System;
using System.Collections.Generic;
using Cubach.Shared;
using Cubach.Shared.Network;

namespace Cubach.Client.Network
{
    public class LocalClientManager : IClientManager
    {
        private static readonly Random Random = new Random();

        private readonly Dictionary<long, LocalClientConnection> connections =
            new Dictionary<long, LocalClientConnection>();

        public event EventHandler<ErrorEventArgs> Error = (s, e) => { };
        public event EventHandler<ClientConnectionEventArgs> Connected = (s, e) => { };
        public event EventHandler<ClientConnectionEventArgs> Disconnected = (s, e) => { };
        public event EventHandler<ClientConnectionEventArgs> ChunksRequestReceived = (s, e) => { };

        public void AddConnection(Queue<ServerMessage> serverQueue, Queue<ClientMessage> clientQueue)
        {
            var id = Random.Next();
            var connection = new LocalClientConnection(id, clientQueue, serverQueue);
            connections.Add(id, connection);
        }

        public void Run()
        {
            foreach (var connection in connections.Values)
            {
                Connected(this, new ClientConnectionEventArgs(connection));
            }
        }

        private void ProcessMessages(LocalClientConnection connection)
        {
            while (connection.IncomingMessages.Count > 0)
            {
                var msg = connection.IncomingMessages.Dequeue();
                if (msg is ChunksRequest)
                {
                    ChunksRequestReceived(this, new ClientConnectionEventArgs(connection));
                }
            }
        }

        public void ProcessMessages()
        {
            foreach (var connection in connections.Values)
            {
                ProcessMessages(connection);
            }
        }

        public void SendChunk(Chunk chunk)
        {
            foreach (var connection in connections.Values)
            {
                connection.SendChunk(chunk);
            }
        }
    }
}
