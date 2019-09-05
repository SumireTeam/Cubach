using System;
using System.Runtime.InteropServices;

namespace Cubach.Model
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Block
    {
        public static readonly int SizeInBytes = Marshal.SizeOf<Block>();

        public readonly int TypeID;

        public Block(int typeId) => TypeID = typeId;

        public BlockType Type => BlockType.GetById(TypeID);
        public string Name => Type.Name;
        public BlockTextures Textures => Type.Textures;
        public bool Solid => Type.Solid;
        public BlockTransparency Transparency => Type.Transparency;

        public bool Opaque => Transparency == BlockTransparency.Opaque;
        public bool SemiTransparent => Transparency == BlockTransparency.SemiTransparent;
        public bool Transparent => Transparency == BlockTransparency.Transparent;

        public byte[] GetBytes()
        {
            return BitConverter.GetBytes(TypeID);
        }

        public static Block Create(byte[] bytes, int index = 0)
        {
            var typeId = BitConverter.ToInt32(bytes, index);
            return new Block(typeId);
        }
    }
}
