using System;
using System.Collections.Generic;

namespace PassivePicasso.ThunderKit.Core.Editor
{
    public static class Extensions
    {
        public static IEnumerable<T> GetFlags<T>(this T input) where T : struct, Enum
        {
            foreach (T value in (T[])Enum.GetValues(typeof(T)))
                if (input.HasFlag(value))
                    yield return value;
        }
    }
}