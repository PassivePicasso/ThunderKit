using System;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_2019
using UnityEngine.UIElements;
#elif UNITY_2018
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Config
{
    [Serializable]
    public abstract class OptionalExecutor : Executor
    {
        public bool enabled;
    }
}