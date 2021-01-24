
using PassivePicasso.ThunderKit.Core.Pipelines;
using System.Diagnostics;
using System.Text;

namespace ThunderKit.Core.Pipelines.Composable.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class ExecuteProcess : PipelineJob
    {
        public string workingDirectory;
        public string executable;
        public string[] arguments;

        public override void Execute(Pipeline pipeline)
        {
            var args = new StringBuilder();
            for (int i = 0; i < arguments.Length; i++)
            {
                args.Append(PathReference.ResolvePath(arguments[i], null, pipeline));
                args.Append(" ");
            }

            var exe = PathReference.ResolvePath(executable, null, pipeline);
            var pwd = PathReference.ResolvePath(workingDirectory, null, pipeline);
            var rorPsi = new ProcessStartInfo(exe)
            {
                WorkingDirectory = pwd,
                Arguments = args.ToString(),
                //Standard output redirection doesn't currently work with bepinex, appears to be considered a bepinex bug
                //RedirectStandardOutput = true,
                UseShellExecute = true
            };

            Process.Start(rorPsi);
        }
    }
}
