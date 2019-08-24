using System.Runtime.InteropServices;

namespace Cubach.Model
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Block
    {
        public readonly int Type;

        public Block(int type)
        {
            Type = type;
        }
    }
}
