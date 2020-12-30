#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using PassivePicasso.ThunderKit.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class BuildManifestAssetBundles : PipelineJob
    {
        [EnumFlag]
        public BuildAssetBundleOptions AssetBundleBuildOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        public bool recurseDirectories;
        public bool simulate;

        public override void Execute(Pipeline pipeline)
        {
            var manifestPipeline = pipeline as ManifestPipeline;
            var manifest = manifestPipeline.Manifest;
            if (manifest?.assetBundles?.Any() != true) return;

            AssetDatabase.SaveAssets();

            var playerAssemblies = CompilationPipeline.GetAssemblies();
            var assemblyFiles = playerAssemblies.Select(pa => pa.outputPath).ToArray();
            var sourceFiles = playerAssemblies.SelectMany(pa => pa.sourceFiles).ToArray();
            var excludedExtensions = new[] { ".dll" };


            bool IsManifest(string dependency, Manifest man) => $"{man.author}-{man.name}-{man.version_number}".Equals(dependency);
            var dependantManifests = manifest.dependencies
                .Select(dep => manifestPipeline.manifests.FirstOrDefault(man => IsManifest(dep, man)))
                .Where(dep => dep != null);
            var explicitDownstreamAssets = dependantManifests.SelectMany(man => man.assetBundles).SelectMany(mab => mab.assets).Select(asset => AssetDatabase.GetAssetPath(asset)).ToArray();


            var builds = new AssetBundleBuild[manifest.assetBundles.Length];
            var fileCount = 0;

            var output/**/ = Path.Combine(pipeline.OutputRoot, pipeline.name);
            var bepinexDir = Path.Combine(output, "BepInExPack", "BepInEx");
            var targetPath = Path.Combine(bepinexDir, "plugins", manifest.name);

            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);


            var logBuilder = new StringBuilder();
            logBuilder.AppendLine("Constructing AssetBundles");

            for (int i = 0; i < manifest.assetBundles.Length; i++)
            {
                var def = manifest.assetBundles[i];
                var explicitAssets = manifest.assetBundles.Where((ab, abi) => abi != i).SelectMany(ab => ab.assets).Select(asset => AssetDatabase.GetAssetPath(asset)).ToArray();
                var build = builds[i];
                var assets = new List<string>();
                logBuilder.AppendLine($"Building bundle: {def.assetBundleName}");
                foreach (var asset in def.assets)
                {
                    var assetPath = AssetDatabase.GetAssetPath(asset);
                    bool isFolder = AssetDatabase.IsValidFolder(assetPath);
                    
                    logBuilder.AppendLine($"Asset: {asset.name} is a {(isFolder ? "Folder" : "File")}");
                    if (isFolder)
                    {
                        var bundleAssets = AssetDatabase.GetAllAssetPaths()
                            .Where(ap => !AssetDatabase.IsValidFolder(ap))
                            .Where(ap => ap.StartsWith(assetPath))
                            .Where(ap => !assets.Contains(ap))
                            .Where(ap => !sourceFiles.Contains(ap))
                            .Where(ap => !assemblyFiles.Contains(ap))
                            .Where(ap => recurseDirectories || Path.GetDirectoryName(ap).Replace('\\', '/').Equals(assetPath))
                            .SelectMany(ap => AssetDatabase.GetDependencies(ap)
                                                           .Where(dap => !explicitAssets.Contains(dap))
                                                           .Where(dap => !explicitDownstreamAssets.Contains(dap))
                                        )
                            .Where(ap =>
                            {
                                var extension = Path.GetExtension(ap);
                                return !excludedExtensions.Contains(extension);
                            })

                            .Where(ap => !assets.Contains(ap))
                            ;
                        assets.AddRange(bundleAssets);
                    }
                    else
                    {
                        var validAssets = AssetDatabase.GetDependencies(assetPath)
                            .Where(dap => !explicitDownstreamAssets.Contains(dap))
                            .Where(dap => !explicitAssets.Contains(dap))
                            .Where(ap => !assets.Contains(ap))
                            .Where(ap =>
                            {
                                var extension = Path.GetExtension(ap);
                                return !excludedExtensions.Contains(extension);
                            })
                            .Where(ap => !sourceFiles.Contains(ap))
                            .Where(ap => !assemblyFiles.Contains(ap))
                            .Where(ap => AssetDatabase.GetMainAssetTypeAtPath(ap) != typeof(UnityPackage))
                            ;
                        assets.AddRange(validAssets);
                    }
                }
                build.assetNames = assets.ToArray();
                build.assetBundleName = def.assetBundleName;
                builds[i] = build;
                fileCount += build.assetNames.Length;
                foreach (var asset in build.assetNames)
                    logBuilder.AppendLine(asset);
            }

            logBuilder.AppendLine($"Constructed {builds.Length} AssetBundleBuilds with {fileCount} files.");

            if (!simulate) BuildPipeline.BuildAssetBundles(targetPath, builds, AssetBundleBuildOptions, buildTarget);

            Debug.Log(logBuilder.ToString());
        }
    }
}
#endif