#if UNITY_EDITOR
using UnityEngine;

namespace RainOfStages.Deploy
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