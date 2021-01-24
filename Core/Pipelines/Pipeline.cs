#if UNITY_EDITOR
using ThunderKit.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;

namespace ThunderKit.Core.Pipelines
{
    public class Pipeline : ComposableObject
    {
        public IEnumerable<PipelineJob> RunSteps => Data.Cast<PipelineJob>();

        public string OutputRoot => System.IO.Path.Combine("ThunderKit");

        public override string ElementTemplate => @"
using ThunderKit.Core.Pipelines;

namespace {0}
{{
    [PipelineSupport(typeof(Pipeline))]
    public class {1} : PipelineJob
    {{
        public override void Execute(Pipeline pipeline)
        {{
        }}
    }}
}}
";

        public virtual void Execute()
        {
            PipelineJob[] runnableSteps = RunSteps.Where(step => 
                                                     step.GetType().GetCustomAttributes()
                                                         .OfType<PipelineSupportAttribute>()
                                                         .Any(psa => psa.HandlesPipeline(this.GetType()))).ToArray();
            foreach (var step in runnableSteps) 
                    step.Execute(this);
        }


        [OnOpenAsset]
        public static bool DoubleClickDeploy(int instanceID, int line)
        {
            if (!(EditorUtility.InstanceIDToObject(instanceID) is Pipeline instance)) return false;

            instance.Execute();

            return true;
        }

        public override bool SupportsType(Type type)
        {
            if (ElementType.IsAssignableFrom(type))
            {
                var customAttributes = type.GetCustomAttributes();
                var pipelineSupportAttributes = customAttributes.OfType<PipelineSupportAttribute>();
                if (pipelineSupportAttributes.Any(psa => psa.HandlesPipeline(GetType())))
                    return true;
            }
            return false;
        }

        public override Type ElementType => typeof(PipelineJob);
    }
}
#endif