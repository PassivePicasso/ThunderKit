using System.Threading.Tasks;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class ApplyPatch : PipelineJob
    {
        public string OldFile;
        public string NewFile;
        public string PatchFile;

        public override Task Execute(Pipeline pipeline)
        {
            BsDiff.BsTool.Patch(OldFile, NewFile, PatchFile);

            return Task.CompletedTask;
        }
    }
}
