namespace AssetRipper.VersionUtilities
{
	/// <summary>
	/// A value type for holding Unity versions
	/// </summary>
	public readonly partial struct UnityVersion
	{
		private const int majorOffset = 48;
		private const int minorOffset = 32;
		private const int buildOffset = 16;
		private const int typeOffset = 8;

		private const ulong byteMask = 0xFFUL;
		private const ulong ushortMask = 0xFFFFUL;

		private readonly ulong m_data;

		/// <summary>
		/// The first number in a Unity version string
		/// </summary>
		public ushort Major => unchecked((ushort)((m_data >> majorOffset) & ushortMask));

		/// <summary>
		/// The second number in a Unity version string
		/// </summary>
		public ushort Minor => unchecked((ushort)((m_data >> minorOffset) & ushortMask));

		/// <summary>
		/// The third number in a Unity version string
		/// </summary>
		public ushort Build => unchecked((ushort)((m_data >> buildOffset) & ushortMask));

		/// <summary>
		/// The letter in a Unity version string
		/// </summary>
		public UnityVersionType Type => (UnityVersionType)unchecked((byte)((m_data >> typeOffset) & byteMask));

		/// <summary>
		/// The last number in a Unity version string
		/// </summary>
		public byte TypeNumber => unchecked((byte)(m_data & byteMask));

		/// <summary>
		/// The minimum value this type can have
		/// </summary>
		public static UnityVersion MinVersion { get; } = new UnityVersion(0UL);

		/// <summary>
		/// The maximum value this type can have
		/// </summary>
		public static UnityVersion MaxVersion { get; } = new UnityVersion(ulong.MaxValue);

		/// <summary>
		/// Construct a new Unity version
		/// </summary>
		public UnityVersion(ushort major)
		{
			m_data = (ulong)major << majorOffset;
		}

		/// <summary>
		/// Construct a new Unity version
		/// </summary>
		public UnityVersion(ushort major, ushort minor)
		{
			m_data = ((ulong)major << majorOffset) | ((ulong)minor << minorOffset);
		}

		/// <summary>
		/// Construct a new Unity version
		/// </summary>
		public UnityVersion(ushort major, ushort minor, ushort build)
		{
			m_data = ((ulong)major << majorOffset) | ((ulong)minor << minorOffset) | ((ulong)build << buildOffset);
		}

		/// <summary>
		/// Construct a new Unity version
		/// </summary>
		public UnityVersion(ushort major, ushort minor, ushort build, UnityVersionType type)
		{
			m_data = ((ulong)major << majorOffset) | ((ulong)minor << minorOffset) | ((ulong)build << buildOffset)
				| ((ulong)type << typeOffset);
		}

		/// <summary>
		/// Construct a new Unity version
		/// </summary>
		public UnityVersion(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber)
		{
			m_data = ((ulong)major << majorOffset) | ((ulong)minor << minorOffset) | ((ulong)build << buildOffset)
				| ((ulong)type << typeOffset) | typeNumber;
		}

		private UnityVersion(ulong data)
		{
			m_data = data;
		}
	}
}
