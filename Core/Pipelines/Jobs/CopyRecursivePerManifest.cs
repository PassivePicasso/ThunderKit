using ThunderKit.Core.Attributes;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor]
    public class CopyRecursivePerManifest : CopyRecursive
    {
        public override void Execute(Pipeline pipeline)
        {
        }
    }
}
