namespace AssetRipper.VersionUtilities
{
	public readonly partial struct UnityVersion
	{
		/// <summary>
		/// Equality operator
		/// </summary>
		/// <param name="left">The left Unity version</param>
		/// <param name="right">The right Unity version</param>
		/// <returns></returns>
		public static bool operator ==(UnityVersion left, UnityVersion right)
		{
			return left.m_data == right.m_data;
		}

		/// <summary>
		/// Inequality operator
		/// </summary>
		/// <param name="left">The left Unity version</param>
		/// <param name="right">The right Unity version</param>
		/// <returns></returns>
		public static bool operator !=(UnityVersion left, UnityVersion right)
		{
			return left.m_data != right.m_data;
		}

		/// <summary>
		/// Greater than
		/// </summary>
		/// <param name="left">The left Unity version</param>
		/// <param name="right">The right Unity version</param>
		/// <returns></returns>
		public static bool operator >(UnityVersion left, UnityVersion right)
		{
			return left.m_data > right.m_data;
		}

		/// <summary>
		/// Greater than or equal to
		/// </summary>
		/// <param name="left">The left Unity version</param>
		/// <param name="right">The right Unity version</param>
		/// <returns></returns>
		public static bool operator >=(UnityVersion left, UnityVersion right)
		{
			return left.m_data >= right.m_data;
		}

		/// <summary>
		/// Less than
		/// </summary>
		/// <param name="left">The left Unity version</param>
		/// <param name="right">The right Unity version</param>
		/// <returns></returns>
		public static bool operator <(UnityVersion left, UnityVersion right)
		{
			return left.m_data < right.m_data;
		}

		/// <summary>
		/// Less than or equal to
		/// </summary>
		/// <param name="left">The left Unity version</param>
		/// <param name="right">The right Unity version</param>
		/// <returns></returns>
		public static bool operator <=(UnityVersion left, UnityVersion right)
		{
			return left.m_data <= right.m_data;
		}
	}
}
