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
                    var patcher = Path.Combine(modDir, "patchers");
                    var plugins = Path.Combine(modDir, "plugins");
                    var monomod = Path.Combine(modDir, "monomod");
                    var patchersOutput = Path.Combine(bepinexDir, "patchers");
                    var pluginsOutput = Path.Combine(bepinexDir, "plugins");
                    var monomodOutput = Path.Combine(bepinexDir, "monomod");
                    var patchersModOutput = Path.Combine(bepinexDir, "patchers", modDir);
                    var pluginsModOutput = Path.Combine(bepinexDir, "plugins", modDir);
                    var monomodModOutput = Path.Combine(bepinexDir, "monomod", modDir);

                    if (Directory.Exists(patchersModOutput)) Directory.Delete(patchersModOutput);
                    if (Directory.Exists(pluginsModOutput)) Directory.Delete(pluginsModOutput);
                    if (Directory.Exists(monomodModOutput)) Directory.Delete(monomodModOutput);

                    if (!Directory.Exists(patcher) && !Directory.Exists(plugins) && !Directory.Exists(monomod))
                        CopyFilesRecursively(modDir, pluginsOutput);
                    else
                    {
                        if (Directory.Exists(patcher)) CopyFilesRecursively(patcher, patchersOutput);
                        if (Directory.Exists(plugins)) CopyFilesRecursively(plugins, pluginsOutput);
                        if (Directory.Exists(monomod)) CopyFilesRecursively(monomod, monomodOutput);
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