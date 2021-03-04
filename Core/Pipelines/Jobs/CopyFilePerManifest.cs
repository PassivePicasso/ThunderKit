using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class CopyFilePerManifest : PipelineJob
    {
        public Manifests.Manifest[] ExcludedManifests;
        [PathReferenceResolver]
        public string SourceFile;
        [PathReferenceResolver]
        public string DestinationFile;

        public override void Execute(Pipeline pipeline)
        {
            for (pipeline.ManifestIndex = 0; pipeline.ManifestIndex < pipeline.manifests.Length; pipeline.ManifestIndex++)
            {
                if (ExcludedManifests.Contains(pipeline.Manifest)) continue;
                var sourceFile = SourceFile.Resolve(pipeline, this);
                var destinationFile = DestinationFile.Resolve(pipeline, this);

                File.Copy(sourceFile, destinationFile, true);
            }
            pipeline.ManifestIndex = -1;
        }
    }
}
