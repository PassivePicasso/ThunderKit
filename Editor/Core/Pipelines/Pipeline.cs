﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThunderKit.Common;
using ThunderKit.Common.Logging;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Utilities;
using ThunderKit.Core.Windows;
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
        public static async void BatchModeExecutePipeline()
        {
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            var args = Environment.GetCommandLineArgs();
            var pipelinePath = string.Empty;
            var manifestPath = string.Empty;
            var showLogWindow = false;
            foreach (var arg in args)
                switch (arg)
                {
                    case string pipelineArg when arg.StartsWith("-pipeline="):
                        pipelinePath = pipelineArg.Substring("-pipeline=".Length);
                        break;
                    case string manifestArg when arg.StartsWith("-manifest="):
                        manifestPath = manifestArg.Substring("-manifest=".Length);
                        break;
                    case string manifestArg when arg.StartsWith("-show-log-window"):
                        showLogWindow = true;
                        break;
                }

            var reenableLogWindow = false;
            if (Application.isBatchMode)
            {
                if (settings.ShowLogWindow && !showLogWindow)
                {
                    settings.ShowLogWindow = false;
                    reenableLogWindow = true;
                }
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<Pipeline>(pipelinePath);
            var manifest = AssetDatabase.LoadAssetAtPath<Manifest>(manifestPath);
            var task = RunPipelineWithManifest(pipeline, manifest);
            while (!task.IsCompleted)
            {
                await Task.Delay(1000);
            }
            if (reenableLogWindow)
                settings.ShowLogWindow = true;

            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
        public static async Task RunPipelineWithManifest(Pipeline pipeline, Manifest manifest)
        {
            // pipeline.manifest is the correct field to use, stop checking every time.
            // pipeline.manifest is the manifest that is assigned to the pipeline containing this job via the editor
            var originalManifest = pipeline.manifest;
            try
            {
                if (manifest)
                {
                    pipeline.manifest = manifest;
                    await pipeline.Execute();
                }
                else
                    await pipeline.Execute();
            }
            finally
            {
                pipeline.manifest = originalManifest;
            }
        }

        [MenuItem(Constants.ThunderKitContextRoot + nameof(Pipeline), false, priority = Constants.ThunderKitMenuPriority)]
        public static void Create() => ScriptableHelper.SelectNewAsset<Pipeline>();

        public bool QuickAccess;

        public Manifest manifest;

        public Manifest[] Manifests { get; private set; }
        public IEnumerable<ManifestDatum> Datums => Manifests.SelectMany(manifest => manifest.Data.OfType<ManifestDatum>());

        public IEnumerable<PipelineJob> Jobs => Data.OfType<PipelineJob>();

        public string OutputRoot => "ThunderKit";

        public override string ElementTemplate =>
@"using System.Threading.Tasks;
using ThunderKit.Core.Pipelines;

namespace {0}
{{
    [PipelineSupport(typeof(Pipeline))]
    public class {1} : PipelineJob
    {{
        public override Task Execute(Pipeline pipeline)
        {{
            return Task.CompletedTask;
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

        string ProgressTitle
        {
            get

            {
                string title = $"Pipeline: {name}";
                var isManifestActive = Manifests != null && Manifests.Length > 0 && ManifestIndex > -1;
                title += isManifestActive ? $"Manifest: {Manifests[ManifestIndex]?.Identity?.name ?? "Error Manifest Not Found"}" : string.Empty;
                return title;
            }
        }

        public string manifestPath => UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(Manifest));
        public string pipelinePath => UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(this));
        public string pipelineLink
        {
            get
            {
                var result = "[";
                result += name;
                if (JobIndex > -1)
                    result += $"({JobIndex} - {Job().name})";
                result += $"](assetlink://{pipelinePath})";
                if (ManifestIndex > -1 && Manifests != null && Manifests.Length > 0)
                    result += $"[{Manifest?.name}](assetlink://{manifestPath})";
                return result;
            }
        }

        public virtual async Task Execute()
        {
            bool isRoot = false;
            try
            {
                if (!Logger)
                {
                    Logger = PipelineLog.CreateLog(this);
                    isRoot = true;
                }

                using (progressBar = new ProgressBar())
                {
                    //currentJobs needs to be populated first to ensure that it is available for logging.
                    currentJobs = Jobs.Where(SupportsType).ToArray();
                    //Needs to run after CurrentJobs is populated
                    Manifests = manifest?.EnumerateManifests()?.Distinct()?.ToArray();
                    Log(Information, $"Execute Pipeline");

                    var manifestJobs = currentJobs.Where(j => j.GetType().GetCustomAttributes(true).OfType<ManifestProcessorAttribute>().Any())
                               .Union(currentJobs.OfType<FlowPipelineJob>().Where(fpj => fpj.PerManifest)).ToArray();

                    if (manifestJobs.Length > 0 && (Manifests == null || Manifests.Length == 0))
                    {
                        var message = $"Pipeline has PipelineJobs that require a Manifest but no Manifest is assigned";
                        var context = manifestJobs.Select(mj => $"[{name}.{mj.GetType().Name}](assetlink://{pipelinePath})")
                                                        .Prepend("").Prepend("Requirements").ToArray();
                        Log(Error, message, context);
                        Log(Error, $"Halted execution");
                        throw new InvalidOperationException(message);
                    }

                    progressBar.Update(title: ProgressTitle);
                    Log(Information, $"Clearing PipelineJob error states");
                    for (JobIndex = 0; JobIndex < currentJobs.Length; JobIndex++)
                    {
                        progressBar.Update($"Clearing PipelineJob state: {Job().name}");
                    }

                    for (JobIndex = 0; JobIndex < currentJobs.Length; JobIndex++)
                    {
                        if (!Job().Active) continue;

                        Log(Information, $"Execute Job");
                        progressBar.Update($"Executing PipelineJob {Job().name}");
                        if (JobIsManifestProcessor())
                            await ExecuteManifestLoop();
                        else
                            await ExecuteJob();
                    }
                    ManifestIndex =
                    JobIndex = -1;
                }
                Log(Information, $"Finished execution");
            }
            catch (Exception e)
            {
                if (!isRoot)
                    throw;
                else
                {
                    var exceptionList = new List<Exception>();
                    var ex = e;
                    while (ex != null)
                    {
                        exceptionList.Add(ex);
                        ex = ex.InnerException;
                    }
                    exceptionList.Reverse();
                    foreach (var exception in exceptionList)
                    {
                        var stackTrace = exception.StackTrace.Replace("\r\n", "\r\n\r\n");
                        var sourceEx = new Regex("in (?<path>[^<>]+?):(?<linenumber>\\d+)");
                        stackTrace = $"Stacktrace\r\n{exception.Message}\r\n\r\n" + sourceEx.Replace(stackTrace, $"in [${{path}}:${{linenumber}}]({ExceptionScheme}://${{path}}#${{linenumber}})");
                        Log(Error, $"{exception.Message}", stackTrace);
                    }
                    var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
                    if (settings.ShowLogWindow)
                        PipelineLogWindow.ShowLog(Logger);
                }
            }
            finally
            {
                Logger = null;
                if (!isRoot)
                    AssetDatabase.SaveAssets();
            }
        }

        public void Log(LogLevel logLevel, string message, params string[] context)
        {
            var contextualMessage = $"{pipelineLink} {message}";
            LogEntry entry = new LogEntry(logLevel, DateTime.Now, contextualMessage, context);
            Logger.Log(entry);
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