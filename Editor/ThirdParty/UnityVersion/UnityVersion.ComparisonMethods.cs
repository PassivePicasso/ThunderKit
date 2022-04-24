namespace AssetRipper.VersionUtilities
{
	public readonly partial struct UnityVersion
	{
		private const ulong subMajorMask = 0x0000FFFFFFFFFFFFUL;
		private const ulong subMinorMask = 0x00000000FFFFFFFFUL;
		private const ulong subBuildMask = 0x000000000000FFFFUL;
		private const ulong subTypeMask = 0x00000000000000FFUL;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public bool IsEqual(ushort major) => this == From(major);
		public bool IsEqual(ushort major, ushort minor) => this == From(major, minor);
		public bool IsEqual(ushort major, ushort minor, ushort build) => this == From(major, minor, build);
		public bool IsEqual(ushort major, ushort minor, ushort build, UnityVersionType type) => this == From(major, minor, build, type);
		public bool IsEqual(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber) => this == new UnityVersion(major, minor, build, type, typeNumber);
		public bool IsEqual(string version) => this == Parse(version);

		public bool IsLess(ushort major) => this < From(major);
		public bool IsLess(ushort major, ushort minor) => this < From(major, minor);
		public bool IsLess(ushort major, ushort minor, ushort build) => this < From(major, minor, build);
		public bool IsLess(ushort major, ushort minor, ushort build, UnityVersionType type) => this < From(major, minor, build, type);
		public bool IsLess(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber) => this < new UnityVersion(major, minor, build, type, typeNumber);
		public bool IsLess(string version) => this < Parse(version);

		public bool IsLessEqual(ushort major) => this <= From(major);
		public bool IsLessEqual(ushort major, ushort minor) => this <= From(major, minor);
		public bool IsLessEqual(ushort major, ushort minor, ushort build) => this <= From(major, minor, build);
		public bool IsLessEqual(ushort major, ushort minor, ushort build, UnityVersionType type) => this <= From(major, minor, build, type);
		public bool IsLessEqual(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber) => this <= new UnityVersion(major, minor, build, type, typeNumber);
		public bool IsLessEqual(string version) => this <= Parse(version);

		public bool IsGreater(ushort major) => this > From(major);
		public bool IsGreater(ushort major, ushort minor) => this > From(major, minor);
		public bool IsGreater(ushort major, ushort minor, ushort build) => this > From(major, minor, build);
		public bool IsGreater(ushort major, ushort minor, ushort build, UnityVersionType type) => this > From(major, minor, build, type);
		public bool IsGreater(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber) => this > new UnityVersion(major, minor, build, type, typeNumber);
		public bool IsGreater(string version) => this > Parse(version);

		public bool IsGreaterEqual(ushort major) => this >= From(major);
		public bool IsGreaterEqual(ushort major, ushort minor) => this >= From(major, minor);
		public bool IsGreaterEqual(ushort major, ushort minor, ushort build) => this >= From(major, minor, build);
		public bool IsGreaterEqual(ushort major, ushort minor, ushort build, UnityVersionType type) => this >= From(major, minor, build, type);
		public bool IsGreaterEqual(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber) => this >= new UnityVersion(major, minor, build, type, typeNumber);
		public bool IsGreaterEqual(string version) => this >= Parse(version);
		
		private UnityVersion From(ushort major)
		{
			ulong data = ((ulong)major << majorOffset) | subMajorMask & m_data;
			return new UnityVersion(data);
		}
		private UnityVersion From(ushort major, ushort minor)
		{
			ulong data = ((ulong)major << majorOffset) | ((ulong)minor << minorOffset) | subMinorMask & m_data;
			return new UnityVersion(data);
		}
		private UnityVersion From(ushort major, ushort minor, ushort build)
		{
			ulong data = ((ulong)major << majorOffset) | ((ulong)minor << minorOffset) | ((ulong)build << buildOffset) |
				subBuildMask & m_data;
			return new UnityVersion(data);
		}
		private UnityVersion From(ushort major, ushort minor, ushort build, UnityVersionType type)
		{
			ulong data = ((ulong)major << majorOffset) | ((ulong)minor << minorOffset) | ((ulong)build << buildOffset) |
				((ulong)(ushort)type << typeOffset) | subTypeMask & m_data;
			return new UnityVersion(data);
		}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
