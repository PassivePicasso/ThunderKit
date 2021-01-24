using System.IO;

namespace ThunderKit.Core.Pipelines.Composable.Jobs
{
    [PipelineSupport(typeof(ComposableManifestPipeline))]
    public class CopyRecursive : PipelineJob
    {
        public string Input;
        public string Output;
        public override void Execute(Pipeline pipeline)
        {
            var composablePipeline = pipeline as ComposableManifestPipeline;
            foreach (var manifest in composablePipeline.manifests)
            {
                string source = PathReference.ResolvePath(Input, manifest, pipeline);
                string destination = PathReference.ResolvePath(Output, manifest, pipeline);
                Directory.CreateDirectory(destination);
                CopyFilesRecursively(source, destination);
            }
        }

        public static void CopyFilesRecursively(string source, string destination)
        {
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }
    }
}
