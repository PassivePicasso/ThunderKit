using System;
using UnityEngine;

namespace ThunderKit.Core.Controls
{
    [Serializable]
    class NewScriptInfo : ScriptableObject
    {
        private static NewScriptInfo instance;
        public string scriptPath;
        public bool addAsset;

        public static NewScriptInfo Instance
        {
            get
            {
                if (!instance)
                {
                    instance = ScriptableObject.CreateInstance<NewScriptInfo>();
                }
                return instance;
            }
        }

        void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }

        public void Reset()
        {
            addAsset = false;
            scriptPath = string.Empty;
        }
    }
}