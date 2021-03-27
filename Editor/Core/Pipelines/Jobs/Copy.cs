using System;
using System.IO;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Editor;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class Copy : FlowPipelineJob
    {
        public bool Recursive;
        public bool SourceRequired;

        [PathReferenceResolver]
        public string Source;
        [PathReferenceResolver]
        public string Destination;

        protected override void ExecuteInternal(Pipeline pipeline)
        {
            var source = Source.Resolve(pipeline, this);
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
                if (!Directory.Exists(source)) return;
                else if (sourceIsFile)
                    throw new ArgumentException($"Source Error: Expected Directory, Recieved File {source}");
            }

            if (!sourceIsFile) Directory.CreateDirectory(destination);
            else
                Directory.CreateDirectory(Path.GetDirectoryName(destination));

            if (Recursive) CopyFilesRecursively(pipeline, source, destination);
            else
                File.Copy(source, destination, true);
        }
        public static void CopyFilesRecursively(Pipeline pipeline, string source, string destination)
        {
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            foreach (string newPath in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }
    }
}
