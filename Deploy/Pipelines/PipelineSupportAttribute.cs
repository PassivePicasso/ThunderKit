using System;
using System.Linq;

namespace PassivePicasso.ThunderKit.Deploy.Pipelines
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PipelineSupportAttribute : Attribute
    {
        Type[] pipelineTypes;
        public PipelineSupportAttribute(params Type[] pipelineTypes)
        {
            this.pipelineTypes = pipelineTypes;
        }

        public bool HandlesPipeline(Type pipelineType)
        {
            if (!typeof(Pipeline).IsAssignableFrom(pipelineType)) return false;

            return pipelineTypes.Any(t => t.IsAssignableFrom(pipelineType));
        }
    }
}