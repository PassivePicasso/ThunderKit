using System;
using System.Collections.Generic;
using System.Text;
using AssetsExporter.YAML;

namespace AssetsExporter.YAML.Utils.Extensions
{
	public static class YAMLMappingNodeExtensions
	{
		public static void AddSerializedVersion(this YAMLMappingNode _this, int version)
		{
			if (version > 1)
			{
				_this.Add(SerializedVersionName, version);
			}
		}

		public static void ForceAddSerializedVersion(this YAMLMappingNode _this, int version)
		{
			if (version > 0)
			{
				_this.Add(SerializedVersionName, version);
			}
		}

		public static void InsertSerializedVersion(this YAMLMappingNode _this, int version)
		{
			if (version > 1)
			{
				_this.InsertBegin(SerializedVersionName, version);
			}
		}

		public const string SerializedVersionName = "serializedVersion";
	}
}
