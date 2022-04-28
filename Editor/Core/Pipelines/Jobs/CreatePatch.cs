using System.Threading.Tasks;
using UnityEngine;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class CreatePatch : PipelineJob
    {
        [Tooltip("Assembly that the patch will be applied to")]
        public string OldFile;
        [Tooltip("Assembly that the patch will change Old File into")]
        public string NewFile;
        [Tooltip("Output Patch file that will be used to convert OldFile into NewFile")]
        public string PatchFile;

        public override Task Execute(Pipeline pipeline)
        {
            BsDiff.BsTool.CreateDiff(OldFile, NewFile, PatchFile);

            return Task.CompletedTask;
        }
    }
}
