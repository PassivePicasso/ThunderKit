using System;
using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class Copy : PipelineJob
    {
        public bool PerManifest;
        public bool Recursive;
        public bool ErrorOnSourceNotFound;

        public Manifests.Manifest[] ExcludedManifests;
        [PathReferenceResolver]
        public string Source;
        [PathReferenceResolver]
        public string Destination;

        public override void Execute(Pipeline pipeline)
        {
            if (PerManifest)
            {
                for (pipeline.ManifestIndex = 0;
                     pipeline.ManifestIndex < pipeline.manifests.Length;
                     pipeline.ManifestIndex++)
                {
                    if (ExcludedManifests.Contains(pipeline.Manifest)) continue;

                    //ExecuteCopy  will return early if no source is found to skip to the next manifest.
                    //consider this when making changes here.
                    ExecuteCopy(pipeline);
                }
                pipeline.ManifestIndex = -1;
            }
            else
                ExecuteCopy(pipeline);
        }

        private void ExecuteCopy(Pipeline pipeline)
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
                if (ErrorOnSourceNotFound) throw e;
            }

            var destinationIsFile = !File.GetAttributes(destination).HasFlag(FileAttributes.Directory);

            if (Recursive)
            {
                if (!Directory.Exists(source)) return;
                else if (sourceIsFile)
                    throw new ArgumentException($"Source Error: Expected Directory, Recieved File {source}");
                else if (destinationIsFile)
                    throw new ArgumentException($"Destination Error: Expected Directory, Recieved File {source}");
            }

            if (!destinationIsFile) Directory.CreateDirectory(destination);
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
