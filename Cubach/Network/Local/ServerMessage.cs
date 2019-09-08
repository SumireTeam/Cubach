using Cubach.Model;

namespace Cubach.Network.Local
{
    public abstract class ServerMessage { }

    public class ChunkMessage : ServerMessage
    {
        public readonly Chunk Chunk;

        public ChunkMessage(Chunk chunk) => Chunk = chunk;
    }
}
