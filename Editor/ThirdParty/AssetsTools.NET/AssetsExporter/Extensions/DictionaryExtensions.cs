using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            if (!dict.TryGetValue(key, out var value))
            {
                dict[key] = value = new TValue();
            }
            return value;
        }

        public static T GetOrAdd<T>(this Dictionary<string, object> dict, string key) where T: new()
        {
            if (!dict.TryGetValue(key, out var value))
            {
                dict[key] = value = new T();
            }
            return (T)value;
        }

        public static T TryGet<T>(this Dictionary<string, object> dict, string key, T @default = default)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return (T)value;
            }

            return @default;
        }

        public static TValue TryGet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue @default = default)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }

            return @default;
        }
    }
}
