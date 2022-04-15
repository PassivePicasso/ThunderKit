using System.Collections;
using System.Threading.Tasks;
using ThunderKit.Core.Config;
using UnityEngine;
#if UNITY_2019
using UnityEngine.UIElements;
#elif UNITY_2018
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Config
{
    public abstract class Executor : ImportExtension
    {
        /// <summary>
        /// Executed after the last files and data have been imported into the project, but before 
        /// </summary>
        public abstract void Execute();

        public virtual VisualElement CreateUI() => null;
    }
}