using System;
using System.Linq;

namespace Cubach.Shared
{
    public enum BlockTransparency : byte
    {
        Opaque = 0,
        SemiTransparent,
        Transparent,
    }

    public struct BlockType
    {
        private static readonly BlockType[] Types;

        public readonly string Name;
        public readonly BlockTextures Textures;
        public readonly bool Solid;
        public readonly BlockTransparency Transparency;

        static BlockType()
        {
            Types = new[] {
                new BlockType("Air", new BlockTextures(""), BlockTransparency.Transparent, solid: false),
                new BlockType("Stone", new BlockTextures("stone")),
                new BlockType("Dirt", new BlockTextures("dirt")),
                new BlockType("Grass", new BlockTextures("dirt", "grass_side", "grass")),
                new BlockType("Sand", new BlockTextures("sand")),
                new BlockType("Water", new BlockTextures("water"), BlockTransparency.SemiTransparent, solid: false),
            };
        }

        private BlockType(string name, BlockTextures textures,
            BlockTransparency transparency = BlockTransparency.Opaque, bool solid = true)
        {
            Name = name;
            Textures = textures;
            Solid = solid;
            Transparency = transparency;
        }

        public bool Opaque => Transparency == BlockTransparency.Opaque;
        public bool SemiTransparent => Transparency == BlockTransparency.SemiTransparent;
        public bool Transparent => Transparency == BlockTransparency.Transparent;

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
}
