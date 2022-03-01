using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetsTools.NET
{
    public struct Hash128
    {
        public byte[] data; //16 bytes
        public Hash128(byte[] data)
        {
            this.data = data;
        }

        public Hash128(IEnumerable<int> data)
        {
            this.data = data.SelectMany(BitConverter.GetBytes).ToArray();
        }

        public Hash128(IEnumerable<uint> data)
        {
            this.data = data.SelectMany(BitConverter.GetBytes).ToArray();
        }

        public Hash128(AssetsFileReader reader)
        {
            data = reader.ReadBytes(16);
        }

        public Guid ToGuid()
        {
            return new Guid(
                ReverseHalfs(data[0]) << 24 |
                ReverseHalfs(data[1]) << 16 |
                ReverseHalfs(data[2]) << 8 |
                ReverseHalfs(data[3]),

                (short)(
                ReverseHalfs(data[4]) << 8 |
                ReverseHalfs(data[5])),
                (short)(
                ReverseHalfs(data[6]) << 8 |
                ReverseHalfs(data[7])),

                ReverseHalfs(data[8]),
                ReverseHalfs(data[9]),
                ReverseHalfs(data[10]),
                ReverseHalfs(data[11]),
                ReverseHalfs(data[12]),
                ReverseHalfs(data[13]),
                ReverseHalfs(data[14]),
                ReverseHalfs(data[15])
            );
        }

        private static byte ReverseHalfs(byte b)
        {
            return (byte)(b >> 4 | b << 4);
        }
    }
}
