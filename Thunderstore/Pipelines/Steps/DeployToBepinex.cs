#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Pipelines;
using System;
using System.IO;
using System.Linq;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class DeployToBepinex : PipelineJob
    {
        [EnumFlag]
        public LogLevel LogLevel;

        public bool ShowConsole;

        public override void Execute(Pipeline pipeline)
        {
            var manifestPipeline = pipeline as ManifestPipeline;
            var manifest = manifestPipeline.Manifest;

            var bepinexName = manifest.dependencies.FirstOrDefault(dep => dep.StartsWith("bbepis-BepInExPack"));
            if (string.IsNullOrEmpty(bepinexName)) return;

            var bepinexSourceDir/*      */= Path.Combine(manifestPipeline.DependenciesPath, bepinexName, "BepInExPack", "BepInEx");
            var bepindexDestinationDir/**/= Path.Combine(manifestPipeline.OutputRoot, "BepInExPack", "BepInEx");
            if (Directory.Exists(bepindexDestinationDir)) Directory.Delete(bepindexDestinationDir, true);
            CopyFilesRecursively(bepinexSourceDir, bepindexDestinationDir);

            if (Directory.Exists(manifestPipeline.ManifestPath))
                DeployMod(bepindexDestinationDir, manifestPipeline.ManifestPath);

            foreach (var modDependency in manifest.dependencies)
            {
                if (modDependency.StartsWith("bbepis-BepInExPack")) continue;

                var dependencyPath = Path.Combine(manifestPipeline.DependenciesPath, modDependency);
                if (Directory.Exists(dependencyPath))
                    DeployMod(bepindexDestinationDir, dependencyPath);
            }

            string configPath = Path.Combine(bepindexDestinationDir, "Config", "BepInEx.cfg");
            if (Directory.Exists(Path.Combine(bepindexDestinationDir, "Config")))
            {
                File.Delete(configPath);
                var logLevels = LogLevel.GetFlags().Select(f => $"{f}").Aggregate((a, b) => $"{a}, {b}");
                string contents = ConfigTemplate.CreatBepInExConfig(ShowConsole, logLevels);
                File.WriteAllText(configPath, contents);
            }
        }
        private static void DeployMod(string bepinexDir, string modDir)
        {
            var monomodExists = TryDeployFeature("monomod", bepinexDir, modDir, CopyModFilesRecursively);
            var patcherExists = TryDeployFeature("patchers", bepinexDir, modDir, CopyModFilesRecursively);
            var pluginsExists = TryDeployFeature("plugins", bepinexDir, modDir, CopyModFilesRecursively);

            if (!patcherExists && !pluginsExists && !monomodExists) CopyModFilesRecursively(modDir, Path.Combine(bepinexDir, "plugins"));
        }

        private static bool TryDeployFeature(string feature, string bepinexDir, string modDir, Action<string, string> refresh)
        {
            var modFolder = Path.Combine(modDir, feature);
            var subFolderOutput = Path.Combine(bepinexDir, feature);

            var modOutput = Path.Combine(bepinexDir, feature, modDir);
            if (Directory.Exists(modOutput)) Directory.Delete(modOutput);

            bool exists = Directory.Exists(modFolder);
            if (exists)
                refresh(modFolder, subFolderOutput);

            return exists;
        }

        public static void CopyFilesRecursively(string source, string destination)
        {
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }

        public static void CopyModFilesRecursively(string source, string destination)
        {
            foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories).Where(f => !".meta".Equals(Path.GetExtension(f))).ToArray())
            {
                var parentDirectory = Path.GetFileName(Path.GetDirectoryName(file));
                var targetParent = Path.GetFileName(destination);
                var subdirectory = parentDirectory.Equals(targetParent) ? destination : Path.Combine(destination, parentDirectory);
                Directory.CreateDirectory(subdirectory);
                File.Copy(file, Path.Combine(subdirectory, Path.GetFileName(file)), true);
            }
        }
    }
}
#endif