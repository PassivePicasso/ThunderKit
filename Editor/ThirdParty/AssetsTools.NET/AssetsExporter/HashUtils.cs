using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AssetsExporter
{
    public static class HashUtils
    {
        public static int ComputeScriptFileID(string @namespace, string className)
        {
            using (var hash = new MD4())
            {
                var idBytes = Encoding.UTF8.GetBytes($"s\0\0\0{@namespace}{className}");
                var hashed = hash.ComputeHash(idBytes);

                var result = 0;

                for (var i = 3; i >= 0; --i)
                {
                    result <<= 8;
                    result |= hashed[i];
                }

                return result;
            }
        }

        public static Guid GetMD4HashGuid(string str)
        {
            using (var md4 = new MD4())
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                var hash = md4.ComputeHash(bytes);
                return new Guid(hash);
            }
        }

        public static Guid GetMD5HashGuid(string str)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.Default.GetBytes(str);
                var hash = md5.ComputeHash(bytes);
                return new Guid(hash);
            }
        }
    }
}
