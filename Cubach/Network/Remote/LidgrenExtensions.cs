using System.IO;
using System.IO.Compression;
using Cubach.Model;
using Lidgren.Network;

namespace Cubach.Network.Remote
{
    public static class LidgrenExtensions
    {
        public static void WriteBytesWithCompression(this NetOutgoingMessage message, byte[] input)
        {
            using (var outputStream = new MemoryStream()) {
                using (var compressionStream = new GZipStream(outputStream, CompressionMode.Compress)) {
                    compressionStream.Write(input, 0, input.Length);
                }

                var output = outputStream.ToArray();
                message.Write(output.Length);
                message.Write(output);
            }
        }

        public static byte[] ReadBytesWithCompression(this NetIncomingMessage message)
        {
            var length = message.ReadInt32();
            var input = message.ReadBytes(length);
            using (var inputStream = new MemoryStream(input))
            using (var compressionStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream()) {
                compressionStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }

        public static void WriteChunk(this NetOutgoingMessage message, Chunk chunk)
        {
            message.Write(chunk.X);
            message.Write(chunk.Y);
            message.Write(chunk.Z);

            var chunkSerializer = new ChunkSerializer();
            message.WriteBytesWithCompression(chunkSerializer.ToByteArray(chunk));
        }

        public static Chunk ReadChunk(this NetIncomingMessage message)
        {
            var x = message.ReadInt32();
            var y = message.ReadInt32();
            var z = message.ReadInt32();
            var bytes = message.ReadBytesWithCompression();

            var chunkSerializer = new ChunkSerializer();
            return chunkSerializer.FromByteArray(x, y, z, bytes);
        }
    }
}
