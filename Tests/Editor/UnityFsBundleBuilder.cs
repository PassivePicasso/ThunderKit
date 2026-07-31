using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ThunderKitTests
{
    // Builds an uncompressed UnityFS bundle in memory so the compressed player layout can
    // be exercised without committing a multi-megabyte data.unity3d fixture. Entry payloads
    // are filler, so entries are locatable but never loadable as serialized files.
    static class UnityFsBundleBuilder
    {
        const int PayloadSize = 64;
        const uint SerializedFileFlag = 4;
        // Block info is uncompressed (low six bits clear) and combined with the directory list.
        const uint UncompressedCombinedInfo = 0x40;

        public static byte[] Build(string engineVersion, IList<string> entryNames)
        {
            var data = new MemoryStream();
            var offsets = new List<long>();
            for (var i = 0; i < entryNames.Count; i++)
            {
                offsets.Add(data.Length);
                var payload = new byte[PayloadSize];
                for (var b = 0; b < payload.Length; b++)
                    payload[b] = (byte)(i + 1);
                data.Write(payload, 0, payload.Length);
            }
            var dataBytes = data.ToArray();
            var blockInfoBytes = BuildBlockAndDirectoryInfo(entryNames, offsets, dataBytes.Length);

            var bundle = new MemoryStream();
            WriteCString(bundle, "UnityFS");
            WriteUInt32(bundle, 6);
            WriteCString(bundle, "5.x.x");
            WriteCString(bundle, engineVersion);

            // The header fields still to be written are a fixed 20 bytes.
            var totalSize = bundle.Length + 20 + blockInfoBytes.Length + dataBytes.Length;
            WriteInt64(bundle, totalSize);
            WriteUInt32(bundle, (uint)blockInfoBytes.Length);
            WriteUInt32(bundle, (uint)blockInfoBytes.Length);
            WriteUInt32(bundle, UncompressedCombinedInfo);

            bundle.Write(blockInfoBytes, 0, blockInfoBytes.Length);
            bundle.Write(dataBytes, 0, dataBytes.Length);
            return bundle.ToArray();
        }

        static byte[] BuildBlockAndDirectoryInfo(IList<string> entryNames, IList<long> offsets, int dataLength)
        {
            var stream = new MemoryStream();
            stream.Write(new byte[16], 0, 16); // content hash, unused by readers

            WriteUInt32(stream, 1); // a single uncompressed block covering all entries
            WriteUInt32(stream, (uint)dataLength);
            WriteUInt32(stream, (uint)dataLength);
            WriteUInt16(stream, 0);

            WriteUInt32(stream, (uint)entryNames.Count);
            for (var i = 0; i < entryNames.Count; i++)
            {
                WriteInt64(stream, offsets[i]);
                WriteInt64(stream, PayloadSize);
                WriteUInt32(stream, SerializedFileFlag);
                WriteCString(stream, entryNames[i]);
            }
            return stream.ToArray();
        }

        // Bundle headers and block info are big endian regardless of platform.
        static void WriteUInt16(Stream stream, ushort value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        static void WriteUInt32(Stream stream, uint value)
        {
            for (var shift = 24; shift >= 0; shift -= 8)
                stream.WriteByte((byte)(value >> shift));
        }

        static void WriteInt64(Stream stream, long value)
        {
            for (var shift = 56; shift >= 0; shift -= 8)
                stream.WriteByte((byte)(value >> shift));
        }

        static void WriteCString(Stream stream, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
            stream.WriteByte(0);
        }
    }
}
