using AssetRipper.VersionUtilities.Extensions;
using System;
using System.IO;
using System.Text;

namespace AssetRipper.VersionUtilities
{
	public readonly partial struct UnityVersion
	{
		/// <summary>
		/// Serialize the version as a string
		/// </summary>
		/// <returns>A new string like 2019.4.3f1</returns>
		public override string ToString()
		{
			return $"{Major}.{Minor}.{Build}{Type.ToCharacter()}{TypeNumber}";
		}

		/// <summary>
		/// Serialize the version as a string
		/// </summary>
		/// <param name="hasUnderscorePrefix">Include the _ prefix</param>
		/// <param name="useUnderscores">Use underscores as separators instead of periods</param>
		/// <param name="hasExtension">Include the .dll extension</param>
		/// <returns>A new string generated with those parameters</returns>
		public string ToString(bool hasUnderscorePrefix, bool useUnderscores, bool hasExtension)
		{
			StringBuilder sb = new StringBuilder();

			char separator = useUnderscores ? '_' : '.';

			if(hasUnderscorePrefix)
				sb.Append('_');

			sb.Append(Major);
			sb.Append(separator);
			sb.Append(Minor);
			sb.Append(separator);
			sb.Append(Build);
			sb.Append(separator);
			sb.Append(Type.ToCharacter());
			sb.Append(TypeNumber);
			
			if(hasExtension)
				sb.Append(".dll");

			return sb.ToString();
		}

		/// <summary>
		/// Serialize the version as a string
		/// </summary>
		/// <returns>A new string like 2019.4.3</returns>
		public string ToStringWithoutType()
		{
			return $"{Major}.{Minor}.{Build}";
		}

		/// <summary>
		/// Parse a dll name like _2019_4_3f1.dll
		/// </summary>
		/// <param name="dllName">The name of a dll file</param>
		/// <returns>The parsed Unity version</returns>
		/// <exception cref="ArgumentNullException">If the string is null or empty</exception>
		public static UnityVersion ParseFromDllName(string dllName)
		{
			if (string.IsNullOrEmpty(dllName))
			{
				throw new ArgumentNullException(nameof(dllName));
			}
			if (dllName[0] == '_')
			{
				dllName = dllName.Substring(1);
			}

			return Parse(dllName.Replace('_', '.').Replace(".dll", ""));
		}

		/// <summary>
		/// Parse a normal Unity version string
		/// </summary>
		/// <param name="version">A string to parse</param>
		/// <returns>The parsed Unity version</returns>
		/// <exception cref="ArgumentNullException">If the string is null or empty</exception>
		/// <exception cref="ArgumentException">If the string is in an invalid format</exception>
		public static UnityVersion Parse(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				throw new ArgumentNullException(nameof(version));
			}

			int major = 0;
			int minor = 0;
			int build = 0;
			UnityVersionType versionType = UnityVersionType.Final;
			int typeNumber = 0;

			using (StringReader reader = new StringReader(version))

			{
				while (true)
				{
					int symb = reader.Read();
					if (symb == -1)
					{
						throw new ArgumentException($"Invalid version formatting: {version}", nameof(version));
					}
					char c = (char)symb;
					if (c == '.')
					{
						break;
					}

					major = major * 10 + c.ParseDigit();
				}

				while (true)
				{
					int symb = reader.Read();
					if (symb == -1)
					{
						break;
					}
					char c = (char)symb;
					if (c == '.')
					{
						break;
					}

					minor = minor * 10 + c.ParseDigit();
				}

				while (true)
				{
					int symb = reader.Read();
					if (symb == -1)
					{
						break;
					}

					char c = (char)symb;
					if (char.IsDigit(c))
					{
						build = build * 10 + c.ParseDigit();
					}
					else
					{
						versionType = c.ToUnityVersionType();
						break;
					}
				}

				while (true)
				{
					int symb = reader.Read();
					if (symb == -1)
					{
						break;
					}

					char c = (char)symb;
					typeNumber = typeNumber * 10 + c.ParseDigit();
				}

				return new UnityVersion((ushort)major, (ushort)minor, (ushort)build, versionType, (byte)typeNumber);
			}
		}
	}
}
