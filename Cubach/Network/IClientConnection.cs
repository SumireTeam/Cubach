using Cubach.Model;

namespace Cubach.Network
{
    public interface IClientConnection
    {
        long ID { get; }

        void SendChunk(Chunk chunk);
    }
}
