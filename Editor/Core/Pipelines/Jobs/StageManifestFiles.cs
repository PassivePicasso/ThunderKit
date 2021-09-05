using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Manifests.Datum;
using ThunderKit.Core.Paths;
using UnityEditor;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor, RequiresManifestDatumType(typeof(Files))]
    public class StageManifestFiles : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
            var filesDatums = pipeline.Manifest.Data.OfType<Files>().ToArray();

            foreach (var files in filesDatums)
                foreach (var outputPath in files.StagingPaths.Select(path => path.Resolve(pipeline, this)))
                    foreach (var file in files.files)
                    {
                        var sourcePath = AssetDatabase.GetAssetPath(file);
                        string destPath = Path.Combine(outputPath, Path.GetFileName(sourcePath)).Replace("\\", "/");
                        var isDirectory = AssetDatabase.IsValidFolder(sourcePath);
                        if (!isDirectory)
                        {
                            if (!Directory.Exists(Path.GetDirectoryName(destPath)))
                                Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                            FileUtil.ReplaceFile(sourcePath, destPath);
                        }
                        else
                        {
                            if (!Directory.Exists(destPath)) Directory.CreateDirectory(destPath);
                            FileUtil.ReplaceDirectory(sourcePath, destPath);
                        }
                    }
        }
    }
}