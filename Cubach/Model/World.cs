namespace Cubach.Model
{
    public class World
    {
        public const int Length = 8;
        public const int Width = 8;
        public const int Height = 8;

        public readonly Chunk[,,] Chunks = new Chunk[Length, Width, Height];

        public World()
        {
            for (int i = 0; i < Length; ++i) {
                for (int j = 0; j < Width; ++j) {
                    for (int k = 0; k < Height; ++k) {
                        Chunks[i, j, k] = new Chunk();
                    }
                }
            }
        }
    }
}
