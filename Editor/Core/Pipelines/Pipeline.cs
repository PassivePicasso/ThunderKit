using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThunderKit.Common;
using ThunderKit.Common.Logging;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Manifests;
using ThunderKit.Markdown.ObjectRenderers;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;

namespace ThunderKit.Core.Pipelines
{
    using static LogLevel;
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
        const string ExceptionScheme = "exceptionsource";
        [InitializeOnLoadMethod]
        static void RegisterScheme()
        {
            LinkInlineRenderer.RegisterScheme(
                ExceptionScheme,
                link =>
                {
                    var schemeless = link.Substring($"{ExceptionScheme}://".Length);
                    var parts = schemeless.Split('#');
                    var path = parts[0];
                    var linenumber = int.Parse(parts[1]);
                    InternalEditorUtility.OpenFileAtLineExternal(path, linenumber);
                });
        }


        private PipelineJob[] currentJobs;
        public ProgressBar progressBar { get; private set; }

        public int JobIndex { get; protected set; }
        public int ManifestIndex { get; set; }
        public Manifest Manifest => Manifests?[ManifestIndex];
        [HideInInspector, NonSerialized]
        public PipelineLog Logger;

        string ProgressTitle => $"Pipeline: {name}, {(Manifests != null && Manifests.Length > 0 ? $"Manifest: {Manifests[ManifestIndex]?.Identity?.name ?? "Error Manifest Not Found"}" : string.Empty)}";

        public string pipelinePath { get; private set; }
        public string pipelineLink { get; private set; }
        public virtual async Task Execute()
        {
            bool isRoot = false;
            pipelinePath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(this));
            pipelineLink = $"[{name}:](assetlink://{pipelinePath})";
            try
            {
                if (!Logger)
                {
                    Logger = PipelineLog.CreateLog(this);
                    isRoot = true;
                }

                Log(Information, $"Executing ", string.Empty);
                using (progressBar = new ProgressBar())
                {
                    Manifests = manifest?.EnumerateManifests()?.Distinct()?.ToArray();
                    currentJobs = Jobs.Where(SupportsType).ToArray();

                    var manifestJobs = currentJobs.Where(j => j.GetType().GetCustomAttributes(true).OfType<ManifestProcessorAttribute>().Any())
                               .Union(currentJobs.OfType<FlowPipelineJob>().Where(fpj => fpj.PerManifest)).ToArray();

                    if (manifestJobs.Length > 0 && (Manifests == null || Manifests.Length == 0))
                    {
                        var message = $"Pipeline has PipelineJobs that require a Manifest but no Manifest is assigned";
                        Log(Error, message, string.Empty, manifestJobs.Select(mj => $"[{name}.{mj.GetType().Name}](assetlink://{pipelinePath})")
                                                        .Prepend("PipelineJobs requiring Manifests").ToArray());
                        Log(Error, $"Halted execution", string.Empty);
                        throw new InvalidOperationException(message);
                    }

                    ManifestIndex = 0;
                    progressBar.Update(title: ProgressTitle);
                    Log(Information, $"Clearing PipelineJob error states", string.Empty);
                    for (JobIndex = 0; JobIndex < currentJobs.Length; JobIndex++)
                    {
                        progressBar.Update($"Clearing PipelineJob state: {Job().name}");
                        Job().Errored = false;
                        Job().ErrorMessage = string.Empty;
                    }
                    for (JobIndex = 0; JobIndex < currentJobs.Length; JobIndex++)
                    {
                        try
                        {
                            if (!Job().Active) continue;

                            Log(Information, $"Execute  \"{Job().name}\" at index {JobIndex}", string.Empty);
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
                            throw;
                        }
                    }
                    ManifestIndex =
                    JobIndex = -1;
                }
                Log(Information, $"Finished execution", string.Empty);
            }
            catch (Exception e)
            {
                List<string> context = new List<string>();
                var exception = e;
                while (exception != null)
                {
                    context.Add(exception.Message);
                    exception = exception.InnerException;
                }
                var stackTrace = e.StackTrace.Replace("\r\n", "\r\n\r\n");
                var sourceEx = new Regex("in (?<path>[^<>]+?):(?<linenumber>\\d+)");
                stackTrace = $"{e.Message}\r\n\r\n" + sourceEx.Replace(stackTrace, $"in [${{path}}:${{linenumber}}]({ExceptionScheme}://${{path}}#${{linenumber}})");
                context.Insert(0, pipelineLink);
                Log(Error, $"Error invoking \"{Job().name}\" at index {JobIndex}", stackTrace, context.ToArray());
                if (!isRoot)
                    throw;
            }
            finally
            {
                EditorUtility.SetDirty(Logger);
                AssetDatabase.SaveAssets();
                Logger = null;
            }
        }

        public void Log(LogLevel logLevel, string message, string exception, params string[] context)
        {
            var contextualMessage = $"{pipelineLink} {message}";
            LogEntry entry = new LogEntry(logLevel, DateTime.Now, contextualMessage, exception, context);
            Logger.Log(entry);
        }

        public void ClearErrors()
        {
            foreach (var job in Jobs)
            {
                job.Errored = false;
                job.ErrorMessage = string.Empty;
                job.ErrorStacktrace = string.Empty;
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