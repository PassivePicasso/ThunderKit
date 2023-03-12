using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class Copy : FlowPipelineJob
    {
        [Tooltip("While enabled, replace target directory, removing all prior files")]
        [FormerlySerializedAs("StrictDictionaryReplace")]
        public bool Replace = true;

        [Tooltip("While enabled, will copy entire specified directory & subdirectories. Source and destination should be folders")]
        public bool Recursive;

        [Tooltip("While enabled, will error when the source is not found (default: true)")]
        public bool SourceRequired = true;

        [Tooltip("While enabled, copy will create destination directory if it doesn't already exist")]
        public bool EstablishDestination = true;

        [PathReferenceResolver]
        public string Source;

        [PathReferenceResolver]
        public string Destination;

        private string CopyStatement;

        protected override Task ExecuteInternal(Pipeline pipeline)
        {
            CopyStatement = $"``` {Source} ``` to ``` {Destination} ```\r\n\r\n";

            var source = ResolveSource(pipeline);
            if (!ValidateSource(pipeline, source))
                return Task.CompletedTask;

            var isSourceFile = IsSourceFile(source);
            var destination = Destination.Resolve(pipeline, this);

            if (EstablishDestination)
                Directory.CreateDirectory((isSourceFile ? Path.GetDirectoryName(destination) : destination));

            if (Recursive)
                CopyRecursively(pipeline, isSourceFile, source, destination);
            else
            {
                FileUtil.ReplaceFile(source, destination);
                pipeline.Log(LogLevel.Information, $"{CopyStatement}Copied ``` {source} ``` to ``` {destination} ```");
            }


            return Task.CompletedTask;
        }

        private void CopyRecursively(Pipeline pipeline, bool isSourceFile, string source, string destination)
        {
            if (!ValidateRecursiveSource(pipeline, isSourceFile, source))
                return;

            if (Replace)
                ReplaceDirectory(pipeline, source, destination);
            else
                ReplaceFiles(pipeline, source, destination);
        }

        private void ReplaceFiles(Pipeline pipeline, string source, string destination)
        {
            string[] files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
            var copiedFiles = new List<string>();
            foreach (string sourceFilePath in files)
            {
                var destinationFilePath = sourceFilePath.Replace(source, destination);
                var destinationDirectoryPath = Path.GetDirectoryName(destinationFilePath) ?? string.Empty;
                if(!Directory.Exists(destinationDirectoryPath))
                    Directory.CreateDirectory(destinationDirectoryPath);
                FileUtil.ReplaceFile(sourceFilePath, destinationFilePath);

                copiedFiles.Add(destinationFilePath);
            }
            pipeline.Log(LogLevel.Information, $"{CopyStatement}Copied ``` {source} ``` to ``` {destination} ```", copiedFiles.Aggregate("Copied Files", (a, b) => $"{a}\r\n\r\n. {b}"));
        }

        private void ReplaceDirectory(Pipeline pipeline, string source, string destination)
        {
            FileUtil.ReplaceDirectory(source, destination);
            int i = 1;
            var copiedFiles = Directory.EnumerateFiles(destination, "*", SearchOption.AllDirectories)
                .Prepend("Copied Files")
                .Aggregate((a, b) => $"{a}\r\n\r\n {i++}. {b}");
            pipeline.Log(LogLevel.Information, $"{CopyStatement}Copied ``` {source} ``` to ``` {destination} ```", copiedFiles);
        }

        private bool ValidateRecursiveSource(Pipeline pipeline, bool isSourceFile, string source)
        {
            if (!Directory.Exists(source) && !SourceRequired)
            {
                pipeline.Log(LogLevel.Information, $"{CopyStatement} Source not found and is not required, copy skipped");
                return false;
            }
            if (!Directory.Exists(source) && SourceRequired)
                throw new ArgumentException($"{CopyStatement} Source not found and is required");
            if (isSourceFile)
                throw new ArgumentException($"{CopyStatement} Expected Directory for recursive copy, Received file path: {source}");
            return true;
        }

        private string ResolveSource(Pipeline pipeline)
        {
            try
            {
                return Source.Resolve(pipeline, this);
            }
            catch (InvalidOperationException e)
            {
                if (SourceRequired)
                    throw new InvalidOperationException($"{CopyStatement} Failed to resolve source when source is required", e);
                return string.Empty;
            }
        }

        private bool ValidateSource(Pipeline pipeline, string source)
        {
            if (SourceRequired && string.IsNullOrEmpty(source))
                throw new ArgumentException($"{CopyStatement} Required {nameof(Source)} is empty");

            if (!SourceRequired && string.IsNullOrEmpty(source))
            {
                pipeline.Log(LogLevel.Information, $"{CopyStatement} Source not specified and is not required, copy skipped");
                return false;
            }

            return true;
        }

        private bool IsSourceFile(string source)
        {
            try
            {
                return !File.GetAttributes(source).HasFlag(FileAttributes.Directory);
            }
            catch (Exception e)
            {
                if (SourceRequired)
                    throw new InvalidOperationException($"{CopyStatement} Failed to check {nameof(Source)} attributes when {nameof(Source)} is required", e);
                return false;
            }
        }
    }
}