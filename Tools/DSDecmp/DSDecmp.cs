using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDSRom.Tools.DSDecmp.Formats.Nitro;
using NDSRom.Tools.DSDecmp.Formats;
using System.IO;

namespace NDSRom.Tools.DSDecmp
{
    public enum Compression : byte
    {
        LZ10 = 0,
        LZ11 = 1,
        NullCompression = 2,
        RLE = 3,
        Invalid = 0xFF
    }
    public class DSDecmp
    {
        private static CompressionFormat[] CompressionFormats = new CompressionFormat[] { new LZ10(), new LZ11(), new NullCompression(), new RLE() };

        public static Compression GetValidFormat(Stream input)
        {
            for (byte i = 0; i < CompressionFormats.Length; i++)
            {
                if (CompressionFormats[i].Supports(input, input.Length))
                {
                    return (Compression)i;
                }
            }
            return Compression.Invalid;
        }

        #region Decompress
        public static byte[] Decompress(byte[] input)
        {
            return Decompress(input, input.Length);
        }
        public static byte[] Decompress(byte[] input, long length)
        {
            MemoryStream inputStream = new MemoryStream(input);
            return Decompress(inputStream, length, GetValidFormat(inputStream));
        }
        public static byte[] Decompress(byte[] input, long length, Compression compression)
        {
            MemoryStream inputStream = new MemoryStream(input);
            return Decompress(inputStream, length, compression);
        }
        public static byte[] Decompress(Stream input, long length, Compression compression)
        {
            CompressionFormat compressionFormat = CompressionFormats[(byte)compression];
            MemoryStream outputStream = new MemoryStream();
            compressionFormat.Decompress(input, length, outputStream);
            return outputStream.ToArray();
        }
        #endregion

        #region Compress
        public static byte[] Compress(byte[] input, long length, Compression compression)
        {
            MemoryStream inputStream = new MemoryStream(input);
            return Compress(inputStream, length, compression);
            
        }
        public static byte[] Compress(Stream input, long length, Compression compression)
        {
            CompressionFormat compressionFormat = CompressionFormats[(byte)compression];
            MemoryStream outputStream = new MemoryStream();
            compressionFormat.Compress(input, length, outputStream);
            return outputStream.ToArray();
        }
        #endregion
    }
}