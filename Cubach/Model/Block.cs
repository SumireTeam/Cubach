using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cubach.Model
{
    public struct BlockTextures
    {
        public readonly string Rear;
        public readonly string Front;
        public readonly string Left;
        public readonly string Right;
        public readonly string Bottom;
        public readonly string Top;

        public BlockTextures(string rear, string front, string left, string right, string bottom, string top)
        {
            Rear = rear;
            Front = front;
            Left = left;
            Right = right;
            Bottom = bottom;
            Top = top;
        }

        public BlockTextures(string bottom, string side, string top) : this(side, side, side, side, bottom, top) { }
        public BlockTextures(string texture) : this(texture, texture, texture, texture, texture, texture) { }
    }

    public struct BlockType
    {
        private static readonly BlockType[] Types;

        public readonly string Name;
        public readonly BlockTextures Textures;
        public readonly bool Solid;
        public readonly bool Transparent;

        static BlockType()
        {
            Types = new[] {
                new BlockType("Air", new BlockTextures(""), solid: false, transparent: true),
                new BlockType("Stone", new BlockTextures("stone")),
                new BlockType("Dirt", new BlockTextures("dirt")),
                new BlockType("Grass", new BlockTextures("dirt", "grass_side", "grass")),
            };
        }

        private BlockType(string name, BlockTextures textures, bool solid = true, bool transparent = false)
        {
            Name = name;
            Textures = textures;
            Solid = solid;
            Transparent = transparent;
        }

        public static BlockType GetById(int id)
        {
            return Types[id];
        }

        public static BlockType GetByName(string name)
        {
            return Types.First(type => type.Name == name);
        }

        public static int GetIdByName(string name)
        {
            var item = GetByName(name);
            return Array.IndexOf(Types, item);
        }
    }

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
        public bool Transparent => Type.Transparent;

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
