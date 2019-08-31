namespace Cubach.Model
{
    public class Chunk
    {
        public const int Length = 32;
        public const int Width = 32;
        public const int Height = 32;

        public readonly Block[,,] Blocks = new Block[Length, Width, Height];

        public Chunk()
        {
            for (int i = 0; i < Length; ++i) {
                for (int j = 0; j < Width; ++j) {
                    for (int k = 0; k < Height; ++k) {
                        Blocks[i, j, k] = new Block(0);
                    }
                }
            }
        }
    }
}
