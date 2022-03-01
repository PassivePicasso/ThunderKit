using System;

namespace AssetRipper.VersionUtilities
{
    /// <summary>
    /// An enumeration representing the letter in a Unity Version string
    /// </summary>
    public enum UnityVersionType : byte
    {
        /// <summary>
        /// a
        /// </summary>
        Alpha = 0,
        /// <summary>
        /// b
        /// </summary>
        Beta,
        /// <summary>
        /// c
        /// </summary>
        China,
        /// <summary>
        /// f
        /// </summary>
        Final,
        /// <summary>
        /// p
        /// </summary>
        Patch,
        /// <summary>
        /// x
        /// </summary>
        Experimental,

        /// <summary>
        /// The minimum valid value for this enumeration
        /// </summary>
        MinValue = Alpha,
        /// <summary>
        /// The maximum valid value for this enumeration
        /// </summary>
        MaxValue = Experimental,
    }

    /// <summary>
    /// Extension methods for <see cref="UnityVersionType"/>
    /// </summary>
    public static class UnityVersionTypeExtentions
    {
        /// <summary>
        /// Convert to the relevant character
        /// </summary>
        /// <param name="_this">A Unity version type</param>
        /// <returns>The character this value represents</returns>
        /// <exception cref="ArgumentOutOfRangeException">The type is not a valid value</exception>
        [Obsolete("Changed to ToCharacter", true)]
        public static char ToLiteral(this UnityVersionType _this) => _this.ToCharacter();

        /// <summary>
        /// Convert to the relevant character
        /// </summary>
        /// <param name="type">A Unity version type</param>
        /// <returns>The character this value represents</returns>
        /// <exception cref="ArgumentOutOfRangeException">The type is not a valid value</exception>
        public static char ToCharacter(this UnityVersionType type)
        {
            switch (type)
            {
                case UnityVersionType.Alpha: return 'a';
                case UnityVersionType.Beta: return 'b';
                case UnityVersionType.China: return 'c';
                case UnityVersionType.Final: return 'f';
                case UnityVersionType.Patch: return 'p';
                case UnityVersionType.Experimental: return 'x';
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported version type {type}");
            };
        }

    }
}
