using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThunderKit.Common;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Editor;
using ThunderKit.Core.Manifests;
using UnityEditor;
using UnityEditor.Callbacks;

namespace ThunderKit.Core.Pipelines
{
    public class Pipeline : ComposableObject
    {
        [MenuItem(Constants.ThunderKitContextRoot + nameof(Pipeline), false, priority = Constants.ThunderKitMenuPriority)]
        public static void Create() => ScriptableHelper.SelectNewAsset<Pipeline>();

        public ManifestCollection manifests;
        public IEnumerable<ManifestDatum> Datums => manifests.SelectMany(manifest => manifest.Data.OfType<ManifestDatum>());

        public IEnumerable<PipelineJob> Jobs => Data.OfType<PipelineJob>();

        public string OutputRoot => System.IO.Path.Combine("ThunderKit");

        public override string ElementTemplate => 
@"using ThunderKit.Core.Pipelines;

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

        public int JobIndex { get; protected set; }
        public int ManifestIndex { get; set; }
        public Manifest Manifest => manifests[ManifestIndex];

        public virtual void Execute()
        {
            PipelineJob[] jobs = Jobs.Where(SupportsType).ToArray();

            for (JobIndex = 0; JobIndex < jobs.Length; JobIndex++)
                if (JobIsManifestProcessor())
                    ExecuteManifestLoop();
                else
                    ExecuteJob();

            JobIndex = -1;

            PipelineJob Job() => jobs[JobIndex];

            void ExecuteJob() => Job().Execute(this);

            bool JobIsManifestProcessor() => Job().GetType().GetCustomAttributes<ManifestProcessorAttribute>().Any();

            bool CanProcessManifest(RequiresManifestDatumTypeAttribute attribute) => attribute.CanProcessManifest(Manifest);

            bool JobCanProcessManifest() => Job().GetType().GetCustomAttributes<RequiresManifestDatumTypeAttribute>().All(CanProcessManifest);

            void ExecuteManifestLoop()
            {
                for (ManifestIndex = 0; ManifestIndex < manifests.Length; ManifestIndex++)
                    if (Manifest && JobCanProcessManifest())
                        ExecuteJob();

                ManifestIndex = -1;
            }
        }


        [OnOpenAsset]
        public static bool DoubleClickDeploy(int instanceID, int line)
        {
            if (!(EditorUtility.InstanceIDToObject(instanceID) is Pipeline instance)) return false;

            instance.Execute();

            return true;
        }

        public bool SupportsType(PipelineJob job) => SupportsType(job.GetType());
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