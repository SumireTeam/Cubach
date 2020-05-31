namespace Cubach.Shared.Network
{
    public interface IClientConnection
    {
        long ID { get; }

        void SendChunk(Chunk chunk);
    }
}
