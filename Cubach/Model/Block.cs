﻿using System;
using System.Runtime.InteropServices;

namespace Cubach.Model
{
    public struct BlockType
    {
        public readonly string Name;
        public readonly string Texture;
        public readonly bool Solid;
        public readonly bool Transparent;

        public BlockType(string name, string texture, bool solid = true, bool transparent = false)
        {
            Name = name;
            Texture = texture;
            Solid = solid;
            Transparent = transparent;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Block
    {
        public static readonly BlockType[] Types;
        public static readonly int SizeInBytes = Marshal.SizeOf<Block>();

        static Block()
        {
            Types = new[] {
                new BlockType("Air", "", solid: false, transparent: true),
                new BlockType("Stone", "stone"),
                new BlockType("Dirt", "dirt"),
                new BlockType("Grass", "grass"),
            };
        }

        public readonly int TypeID;
        public Block(int typeId) => TypeID = typeId;
        public BlockType Type => Types[TypeID];
        public string Name => Type.Name;
        public string Texture => Type.Texture;
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
