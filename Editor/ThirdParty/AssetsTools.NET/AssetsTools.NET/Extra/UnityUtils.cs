using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public static class UnityUtils
    {
        public static Guid UnityHashToGuid(IEnumerable<byte> bytes)
        {
            return new Guid(new byte[] {
                //Inversed bytes
                ReverseHalfs(bytes.ElementAt(3)),
                ReverseHalfs(bytes.ElementAt(2)),
                ReverseHalfs(bytes.ElementAt(1)),
                ReverseHalfs(bytes.ElementAt(0)),

                //Inversed byte pairs
                ReverseHalfs(bytes.ElementAt(5)),
                ReverseHalfs(bytes.ElementAt(4)),
                ReverseHalfs(bytes.ElementAt(7)),
                ReverseHalfs(bytes.ElementAt(6)),

                //Normal order
                ReverseHalfs(bytes.ElementAt(8)),
                ReverseHalfs(bytes.ElementAt(9)),
                ReverseHalfs(bytes.ElementAt(10)),
                ReverseHalfs(bytes.ElementAt(11)),

                //Normal order
                ReverseHalfs(bytes.ElementAt(12)),
                ReverseHalfs(bytes.ElementAt(13)),
                ReverseHalfs(bytes.ElementAt(14)),
                ReverseHalfs(bytes.ElementAt(15))
            });
        }

        public static Guid UnityHashToGuid(IEnumerable<int> ints)
        {
            return UnityHashToGuid(ints.SelectMany(BitConverter.GetBytes));
        }

        private static byte ReverseHalfs(byte b)
        {
            return (byte)(b >> 4 | b << 4);
        }
    }
}
