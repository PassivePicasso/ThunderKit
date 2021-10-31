using System.Threading.Tasks;
using UnityEngine;

namespace ThunderKit.Core.Pipelines
{
    public abstract class PipelineJob : ComposableElement
    {
        [HideInInspector]
        public bool Active = true;

        public abstract Task Execute(Pipeline pipeline);
    }
}