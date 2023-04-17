using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Manifests.Datum;
using ThunderKit.Core.Paths;
using System.Threading.Tasks;
using UnityEditor;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor, RequiresManifestDatumType(typeof(Files))]
    public class StageManifestFiles : PipelineJob
    {
        public override Task Execute(Pipeline pipeline)
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
                            pipeline.Log(LogLevel.Information, $"staged ``` {sourcePath} ``` in ``` {destPath} ```");
                        }
                        else
                        {
                            if (!Directory.Exists(destPath)) Directory.CreateDirectory(destPath);
                            FileUtil.ReplaceDirectory(sourcePath, destPath);
                            int i = 1;
                            var copiedFiles = Directory.EnumerateFiles(destPath, "*", SearchOption.AllDirectories)
                                .Prepend("Copied Files")
                                .Aggregate((a, b) => $"{a}\r\n\r\n {i++}. {b}");
                            pipeline.Log(LogLevel.Information, $"staged ``` {sourcePath} ``` in ``` {destPath} ```", copiedFiles);
                        }

                        if (files.includeMetaFiles)
                        {
                            var metaSourcePath = sourcePath + ".meta";
                            if (File.Exists(metaSourcePath))
                            {
                                var metaDestPath = Path.Combine(outputPath, Path.GetFileName(metaSourcePath)).Replace("\\", "/");
                                FileUtil.ReplaceFile(metaSourcePath, metaDestPath);
                                pipeline.Log(LogLevel.Information, $"staged ``` {metaSourcePath} ``` in ``` {metaDestPath} ```");
                            }
                        }
                    }

            return Task.CompletedTask;
        }
    }
}