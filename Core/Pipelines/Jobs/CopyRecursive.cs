using System.IO;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class CopyRecursive : PipelineJob
    {
        [PathReferenceResolver]
        public string Input;
        [PathReferenceResolver]
        public string Output;

        public override void Execute(Pipeline pipeline)
        {
            string source = Input.Resolve(pipeline, this);
            string destination = Output.Resolve(pipeline, this);
            Directory.CreateDirectory(destination);
            CopyFilesRecursively(source, destination);
        }

        public static void CopyFilesRecursively(string source, string destination)
        {
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            foreach (string newPath in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }
    }
}
