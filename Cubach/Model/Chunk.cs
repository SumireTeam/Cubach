using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
