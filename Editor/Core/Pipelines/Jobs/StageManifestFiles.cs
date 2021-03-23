using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Manifests.Datum;
using ThunderKit.Core.Paths;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor, RequiresManifestDatumType(typeof(Files))]
    public class StageManifestFiles : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
            var query = from files in pipeline.Manifest.Data.OfType<Files>()
                        from outputPath in files.StagingPaths.Select(path => path.Resolve(pipeline, this))
                        select (files.files, outputPath);

            foreach (var (files, outputPath) in query)
            {
                if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

                foreach (var file in files)
                {
                    switch (file)
                    {
                        case Texture2D texture:
                            var textureAssetPath = AssetDatabase.GetAssetPath(file);
                            File.WriteAllBytes(Path.Combine(outputPath, Path.GetFileName(textureAssetPath)), texture.EncodeToPNG());
                            break;
                        default:
                            var textAssetPath = AssetDatabase.GetAssetPath(file);
                            File.Copy(textAssetPath, Path.Combine(outputPath, Path.GetFileName(textAssetPath)), true);
                            break;
                    }
                }
            }
        }
    }
}