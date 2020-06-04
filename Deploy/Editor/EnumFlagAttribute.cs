#if UNITY_EDITOR
using UnityEngine;

namespace PassivePicasso.ThunderKit.Deploy.Editor
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