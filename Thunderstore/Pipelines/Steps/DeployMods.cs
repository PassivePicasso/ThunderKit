#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using System.IO;
using System.Linq;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class DeployMods : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
            var manifestPipeline = pipeline as ManifestPipeline;
            var bepinexDir/*     */= Path.Combine(pipeline.OutputRoot, "BepInExPack", "BepInEx");

            if (Directory.Exists(manifestPipeline.StagingPath))
            {
                var dependencyDirs = Directory.EnumerateDirectories(manifestPipeline.StagingPath, "*", searchOption: SearchOption.TopDirectoryOnly).ToArray();

                foreach (var modDir in dependencyDirs)
                {
                    string patcher = Path.Combine(modDir, "patchers");
                    string plugins = Path.Combine(modDir, "plugins");
                    string monomod = Path.Combine(modDir, "monomod");

                    if (!Directory.Exists(patcher) && !Directory.Exists(plugins) && !Directory.Exists(monomod))
                    {
                        CopyFilesRecursively(modDir, Path.Combine(bepinexDir, "plugins"));
                    }
                    else
                    {
                        if (Directory.Exists(patcher)) CopyFilesRecursively(patcher, Path.Combine(bepinexDir, "patchers"));
                        if (Directory.Exists(plugins)) CopyFilesRecursively(plugins, Path.Combine(bepinexDir, "plugins"));
                        if (Directory.Exists(monomod)) CopyFilesRecursively(monomod, Path.Combine(bepinexDir, "monomod"));
                    }
                }
            }
        }
        public static void CopyFilesRecursively(string source, string target)
        {
            foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories).Where(f => !f.EndsWith("meta")).ToArray())
            {
                var parentDirectory = Path.GetFileName(Path.GetDirectoryName(file));
                var targetParent = Path.GetFileName(target);
                var subdirectory = parentDirectory.Equals(targetParent) ? target : Path.Combine(target, parentDirectory);
                Directory.CreateDirectory(subdirectory);
                File.Copy(file, Path.Combine(subdirectory, Path.GetFileName(file)), true);
            }
        }
    }
}
#endif