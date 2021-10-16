using System;
using System.Collections.Generic;
using System.Linq;
using ThunderKit.Common;
using ThunderKit.Common.Logging;
using ThunderKit.Core.Attributes;
using ThunderKit.Core;
using ThunderKit.Core.Manifests;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;

namespace ThunderKit.Core.Pipelines
{
    public class Pipeline : ComposableObject
    {
        [MenuItem(Constants.ThunderKitContextRoot + nameof(Pipeline), false, priority = Constants.ThunderKitMenuPriority)]
        public static void Create() => ScriptableHelper.SelectNewAsset<Pipeline>();

        public Manifest manifest;

        public Manifest[] Manifests { get; private set; }
        public IEnumerable<ManifestDatum> Datums => Manifests.SelectMany(manifest => manifest.Data.OfType<ManifestDatum>());

        public IEnumerable<PipelineJob> Jobs => Data.OfType<PipelineJob>();

        public string OutputRoot => "ThunderKit";

        public override string ElementTemplate =>
@"using ThunderKit.Core.Pipelines;

namespace {0}
{{
    [PipelineSupport(typeof(Pipeline))]
    public class {1} : PipelineJob
    {{
        public override Task Execute(Pipeline pipeline)
        {{
        }}
    }}
}}
";
        private PipelineJob[] currentJobs;
        public ProgressBar progressBar { get; private set; }

        public int JobIndex { get; protected set; }
        public int ManifestIndex { get; set; }
        public Manifest Manifest => Manifests?[ManifestIndex];

        string ProgressTitle => $"Pipeline: {name}, {(Manifests != null && Manifests.Length > 0 ? $"Manifest: {Manifests[ManifestIndex]?.Identity?.name ?? "Error Manifest Not Found"}" : string.Empty)}";


        public virtual async Task Execute()
        {
            using (progressBar = new ProgressBar())
            {
                Manifests = manifest?.EnumerateManifests()?.Distinct()?.ToArray();
                currentJobs = Jobs.Where(SupportsType).ToArray();

                if ((currentJobs.Any(j => j.GetType().GetCustomAttributes(true).OfType<ManifestProcessorAttribute>().Any())
                  || currentJobs.OfType<FlowPipelineJob>().Any(fpj => fpj.PerManifest))
                  && (Manifests == null || Manifests.Length == 0))
                {
                    throw new InvalidOperationException($"Pipeline {name} has PipelineJobs that require a Manifest but no Manifest is assigned");
                }

                ManifestIndex = 0;
                progressBar.Update(title: ProgressTitle);
                for (JobIndex = 0; JobIndex < currentJobs.Length; JobIndex++)
                {
                    progressBar.Update($"Clearing PipelineJob state: {Job().name}");
                    Job().Errored = false;
                    Job().ErrorMessage = string.Empty;
                }
                for (JobIndex = 0; JobIndex < currentJobs.Length; JobIndex++)
                    try
                    {
                        if (!Job().Active) continue;
                        progressBar.Update($"Executing PipelineJob {Job().name}");
                        if (JobIsManifestProcessor())
                            await ExecuteManifestLoop();
                        else
                            await ExecuteJob();
                    }
                    catch (Exception e)
                    {
                        Job().Errored = true;
                        Job().ErrorMessage = e.Message;
                        EditorGUIUtility.PingObject(Job());
                        Debug.LogError($"Error Invoking {Job().name} Job on Pipeline {name}\r\n{e}", this);
                        JobIndex = currentJobs.Length;
                        break;
                    }
                ManifestIndex =
                JobIndex = -1;
            }
        }

        PipelineJob Job() => currentJobs[JobIndex];

        async Task ExecuteJob() => await Job().Execute(this);

        bool JobIsManifestProcessor() => Job().GetType().GetCustomAttributes(true).OfType<ManifestProcessorAttribute>().Any();

        bool CanProcessManifest(RequiresManifestDatumTypeAttribute attribute) => attribute.CanProcessManifest(Manifest);

        bool JobCanProcessManifest() => Job().GetType().GetCustomAttributes(true).OfType<RequiresManifestDatumTypeAttribute>().All(CanProcessManifest);

        async Task ExecuteManifestLoop()
        {
            for (ManifestIndex = 0; ManifestIndex < Manifests.Length; ManifestIndex++)
                if (Manifest && JobCanProcessManifest())
                {
                    progressBar?.Update(title: ProgressTitle);
                    await ExecuteJob();
                }

            ManifestIndex = -1;
        }
        public bool SupportsType(PipelineJob job) => SupportsType(job.GetType());
        public override bool SupportsType(Type type)
        {
            if (ElementType.IsAssignableFrom(type))
            {
                var customAttributes = type.GetCustomAttributes(true);
                var pipelineSupportAttributes = customAttributes.OfType<PipelineSupportAttribute>();
                var thisType = GetType();
                if (pipelineSupportAttributes.Any(psa => psa.HandlesPipeline(thisType)))
                    return true;
            }
            return false;
        }

        public override Type ElementType => typeof(PipelineJob);
    }
}