using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ThunderKit.Common
{
    public static class PathExtensions
    {
        public static string Combine(params string[] parts) => parts.Aggregate((a, b) => Path.Combine(a, b));
    }
}
