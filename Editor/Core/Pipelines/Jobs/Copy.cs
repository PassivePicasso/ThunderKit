using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class Copy : FlowPipelineJob
    {
        public bool Recursive;
        [Tooltip("While enabled, will error when the source is not found (default: true)")]
        public bool SourceRequired = true;

        [Tooltip("While enabled, copy will create destination directory if it doesn't already exist")]
        public bool EstablishDestination = true;

        [PathReferenceResolver]
        public string Source;
        [PathReferenceResolver]
        public string Destination;

        protected override Task ExecuteInternal(Pipeline pipeline)
        {
            var copyLink = $"[({pipeline.JobIndex} - Copy)](assetlink://{pipeline.pipelinePath}) ``` {Source} ``` to ``` {Destination} ```\r\n";
            var source = string.Empty;
            try
            {
                source = Source.Resolve(pipeline, this);
            }
            catch (Exception e)
            {
                if (SourceRequired)
                    throw new InvalidOperationException($"{copyLink} Failed to resolve source when source is required", e);
            }
            if (SourceRequired && string.IsNullOrEmpty(source)) throw new ArgumentException($"{copyLink} Required {nameof(Source)} is empty");
            if (!SourceRequired && string.IsNullOrEmpty(source))
            {
                pipeline.Log(LogLevel.Information, $"{copyLink} Source not specified and is not required, copy skipped");
                return Task.CompletedTask;
            }
            var destination = Destination.Resolve(pipeline, this);

            bool sourceIsFile = false;

            try
            {
                sourceIsFile = !File.GetAttributes(source).HasFlag(FileAttributes.Directory);
            }
            catch (Exception e)
            {
                if (SourceRequired)
                    throw new InvalidOperationException($"{copyLink} Failed to check {nameof(Source)} attributes when {nameof(Source)} is required", e);
            }

            if (Recursive)
            {
                if (!Directory.Exists(source) && !SourceRequired)
                {
                    pipeline.Log(LogLevel.Information, $"{copyLink} Source not found and is not required, copy skipped");
                    return Task.CompletedTask;
                }
                else if (!Directory.Exists(source) && SourceRequired)
                {
                    throw new ArgumentException($"{copyLink} Source not found and is required");
                }
                else if (sourceIsFile)
                    throw new ArgumentException($"{copyLink} Expected Directory for recursive copy, Recieved file path: {source}");
            }

            if (EstablishDestination)
                Directory.CreateDirectory(sourceIsFile ? Path.GetDirectoryName(destination) : destination);

            if (Recursive)
            {
                FileUtil.ReplaceDirectory(source, destination);
                int i = 1;
                var copiedFiles = Directory.EnumerateFiles(destination, "*", SearchOption.AllDirectories)
                    .Prepend("Copied Files")
                    .Aggregate((a, b) => $"{a}\r\n\r\n {i++}. {b}");
                pipeline.Log(LogLevel.Information, $"{copyLink}\r\n\r\nCopied ``` {source} ``` to ``` {destination} ```", copiedFiles);
            }
            else
            {
                FileUtil.ReplaceFile(source, destination);
                pipeline.Log(LogLevel.Information, $"{copyLink}\r\n\r\n``` {source} ``` to ``` {destination} ```");
            }

            return Task.CompletedTask;
        }
    }
}
