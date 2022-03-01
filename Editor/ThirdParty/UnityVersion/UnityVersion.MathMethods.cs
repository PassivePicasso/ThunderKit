using System;

namespace AssetRipper.VersionUtilities
{
	public readonly partial struct UnityVersion
	{
		/// <summary>
		/// A maximizing function for Unity versions
		/// </summary>
		/// <param name="left">A Unity version</param>
		/// <param name="right">A Unity version</param>
		/// <returns>The larger Unity version</returns>
		public static UnityVersion Max(UnityVersion left, UnityVersion right)
		{
			return left > right ? left : right;
		}

		/// <summary>
		/// A minimizing function for Unity versions
		/// </summary>
		/// <param name="left">A Unity version</param>
		/// <param name="right">A Unity version</param>
		/// <returns>The smaller Unity version</returns>
		public static UnityVersion Min(UnityVersion left, UnityVersion right)
		{
			return left < right ? left : right;
		}

		/// <summary>
		/// A distance function for measuring version proximity
		/// </summary>
		/// <remarks>
		/// The returned value is ordinal and should not be saved anywhere.
		/// It's only for runtime comparisons, such as finding the closest version in a list.
		/// </remarks>
		/// <param name="left">A Unity version</param>
		/// <param name="right">A Unity version</param>
		/// <returns>
		/// An ordinal number representing the distance between 2 versions. 
		/// A value of zero means they're equal.
		/// </returns>
		public static ulong Distance(UnityVersion left, UnityVersion right)
		{
			return left.m_data < right.m_data 
				? right.m_data - left.m_data 
				: left.m_data - right.m_data;
		}

		/// <summary>
		/// Get the closest Unity version in an array of versions using <see cref="Distance(UnityVersion, UnityVersion)"/>
		/// </summary>
		/// <param name="versions">The Unity version array</param>
		/// <returns>The closest Unity version</returns>
		/// <exception cref="ArgumentNullException">The array is null</exception>
		/// <exception cref="ArgumentException">The array is empty</exception>
		public UnityVersion GetClosestVersion(UnityVersion[] versions)
		{
			if (versions is null)
			{
				throw new ArgumentNullException(nameof(versions));
			}

			if (versions.Length == 0)
			{
				throw new ArgumentException("Length cannot be zero", nameof(versions));
			}

			UnityVersion result = versions[0];
			ulong currentDistance = Distance(this, result);
			for(int i = 1; i < versions.Length; i++)
			{
				ulong newDistance = Distance(this, versions[i]);
				if (newDistance < currentDistance)
				{
					currentDistance = newDistance;
					result = versions[i];
				}
			}

			return result;
		}
	}
}
