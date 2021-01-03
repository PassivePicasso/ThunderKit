#if UNITY_EDITOR
using UnityEngine;

namespace PassivePicasso.ThunderKit.Core
{
	public class EnumFlagAttribute : PropertyAttribute
	{
		public string enumName;

		public EnumFlagAttribute() { }

		public EnumFlagAttribute(string name)
		{
			enumName = name;
		}
	}
}
#endif