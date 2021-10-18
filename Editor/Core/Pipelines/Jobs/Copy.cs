using System;
using System.IO;
using System.Threading.Tasks;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using UnityEditor;
using UnityEngine;

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
            var source = string.Empty;
            try
            {
                source = Source.Resolve(pipeline, this);
            }
            catch (Exception e)
            {
                if (SourceRequired) throw e;
            }
            if (SourceRequired && string.IsNullOrEmpty(source)) throw new ArgumentException($"Required {nameof(Source)} is empty");
            if (!SourceRequired && string.IsNullOrEmpty(source))
                return Task.CompletedTask;
            var destination = Destination.Resolve(pipeline, this);

            bool sourceIsFile = false;

            try
            {
                sourceIsFile = !File.GetAttributes(source).HasFlag(FileAttributes.Directory);
            }
            catch (Exception e)
            {
                if (SourceRequired) throw e;
            }

            if (Recursive)
            {
                if (!Directory.Exists(source))
                    return Task.CompletedTask;
                else if (sourceIsFile)
                    throw new ArgumentException($"Source Error: Expected Directory, Recieved File {source}");
            }
            
            if (EstablishDestination)
                Directory.CreateDirectory(sourceIsFile ? Path.GetDirectoryName(destination) : destination);

            if (Recursive)
            {
                FileUtil.ReplaceDirectory(source, destination);
            }
            else
                FileUtil.ReplaceFile(source, destination);
            return Task.CompletedTask;
        }
    }
}
