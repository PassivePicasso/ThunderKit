using System;

namespace AssetRipper.VersionUtilities.Extensions
{
    /// <summary>
    /// Extensions for <see cref="char"/>
    /// </summary>
    public static class CharacterExtensions
    {
        internal static int ParseDigit(this char _this)
        {
            return _this - '0';
        }

        /// <summary>
        /// Parse a character into a Unity Version Type
        /// </summary>
        /// <param name="c">The character</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">No version type for character</exception>
        public static UnityVersionType ToUnityVersionType(this char c)
        {
            switch (c)
            {
                case 'a': return UnityVersionType.Alpha;
                case 'b': return UnityVersionType.Beta;
                case 'c': return UnityVersionType.China;
                case 'f': return UnityVersionType.Final;
                case 'p': return UnityVersionType.Patch;
                case 'x': return UnityVersionType.Experimental;
                default:
                    throw new ArgumentException($"There is no version type {c}", nameof(c));
            };
        }
    }
}
