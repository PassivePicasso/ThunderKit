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
            var filesDatums = pipeline.Manifest.Data.OfType<Files>().ToArray();

            foreach (var files in filesDatums)
            {
                var resolvedPaths = files.StagingPaths.Select(path => path.Resolve(pipeline, this)).ToArray();

                foreach (var outputPath in resolvedPaths)
                {
                    foreach (var file in files.files)
                    {
                        if (typeof(Texture2D).IsAssignableFrom(file.GetType()))
                        {
                            var texture = file as Texture2D;
                            var textureAssetPath = AssetDatabase.GetAssetPath(file);
                            FileUtil.ReplaceFile(textureAssetPath, Path.Combine(outputPath, Path.GetFileName(textureAssetPath)));
                            //File.WriteAllBytes(Path.Combine(outputPath, Path.GetFileName(textureAssetPath)), texture.EncodeToPNG());
                        }
                        else
                        {
                            var textAssetPath = AssetDatabase.GetAssetPath(file);
                            FileUtil.ReplaceFile(textAssetPath, Path.Combine(outputPath, Path.GetFileName(textAssetPath)));
                        }
                    }
                }
            }
        }
    }
}