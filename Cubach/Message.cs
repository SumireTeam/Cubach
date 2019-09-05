using Cubach.Model;

namespace Cubach
{
    public interface IMessage { }

    public class ChunksRequestMessage : IMessage { }

    public class ChunkMessage : IMessage
    {
        public readonly Chunk Chunk;

        public ChunkMessage(Chunk chunk) => Chunk = chunk;
    }
}
