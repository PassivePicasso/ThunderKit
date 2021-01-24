using System.IO;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class CopyRecursive : PipelineJob
    {
        public string Input;
        public string Output;
        public override void Execute(Pipeline pipeline)
        {
            foreach (var manifest in pipeline.manifests)
            {
                string source = PathReference.ResolvePath(Input, pipeline);
                string destination = PathReference.ResolvePath(Output, pipeline);
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
