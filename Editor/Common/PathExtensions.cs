using System.IO;
using System.Linq;

namespace ThunderKit.Common
{
    public static class PathExtensions
    {
        public static string Combine(params string[] parts) => Path.Combine(parts).Replace("\\", "/");
    }
}
