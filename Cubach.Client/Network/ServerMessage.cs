using Cubach.Shared;

namespace Cubach.Client.Network
{
    public abstract class ServerMessage { }

    public class ChunkMessage : ServerMessage
    {
        public readonly Chunk Chunk;

        public ChunkMessage(Chunk chunk) => Chunk = chunk;
    }
}
