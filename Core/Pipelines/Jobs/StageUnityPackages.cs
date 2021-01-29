using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor]
    public class StageUnityPackages : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
            foreach (var unityPackageDatum in pipeline.Manifest.Data.OfType<UnityPackages>())
            {
                foreach (var outputPath in unityPackageDatum.StagingPaths.Select(path => path.Resolve(pipeline, this)))
                {
                    if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

                    foreach (var unityPackage in unityPackageDatum.unityPackages)
                        UnityPackage.Export(unityPackage, outputPath);
                }
            }
        }
    }
}