#if UNITY_EDITOR
using UnityEngine;

namespace ThunderKit.Core.Pipelines
{
    public abstract class PipelineJob : ScriptableObject
    {
        public const string RunStepsMenu = "Run Steps/";

        public abstract void Execute(Pipeline pipeline);
    }
}
#endif