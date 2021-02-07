using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThunderKit.Core;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;
using ThunderKit.Integrations.Thunderstore.Manifests;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace ThunderKit.Integrations.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor, RequiresManifestDatumType(typeof(ThunderstoreManifest), typeof(AssetBundleDefs))]
    public class StageManifestAssetBundles : PipelineJob
    {
        [EnumFlag]
        public BuildAssetBundleOptions AssetBundleBuildOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        public bool recurseDirectories;
        public bool simulate;
        [PathReferenceResolver]
        public string BundleArtifactPath = "%AssetBundleStaging%";
        public override void Execute(Pipeline pipeline)
        {
            var mani = pipeline.Manifest;
            var allThunderstoreManifests = pipeline.Datums.OfType<ThunderstoreManifest>();
            var thunderstoreManifest = mani.Data.OfType<ThunderstoreManifest>().First();
            var dependencies = thunderstoreManifest.dependencies;

            bool IsDependency(string dependency, ThunderstoreManifest man) => $"{man.author}-{man.name}-{man.versionNumber}".Equals(dependency);

            var dependantManifests = pipeline.manifests
                    .Where(man => man.Data.OfType<ThunderstoreManifest>().Any())
                    .Select(man => (manifest: man, tsManifest: man.Data.OfType<ThunderstoreManifest>().First()))
                    .Where(projection => dependencies.Any(dep => IsDependency(dep, projection.tsManifest)))
                    .Select(projection => projection.manifest);

            var explicitDownstreamAssets = dependantManifests
                .SelectMany(man => man.Data.OfType<AssetBundleDefs>())
                .SelectMany(mabd => mabd.assetBundles)
                .SelectMany(ab => ab.assets)
                .Select(asset => AssetDatabase.GetAssetPath(asset))
                .ToArray();

            AssetDatabase.SaveAssets();
            var assetBundleDefs = mani.Data.OfType<AssetBundleDefs>().ToArray();
            var stablePath = BundleArtifactPath.Resolve(pipeline, this);
            Directory.CreateDirectory(stablePath);

            var forbiddenAssets = new List<string>();
            var forbiddenAssetBundles = AssetDatabase.FindAssets($"t:{nameof(ForbiddenAssetBundle)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
            foreach (var forbiddenAsset in forbiddenAssetBundles)
            {
                if (AssetDatabase.IsValidFolder(forbiddenAsset))
                {
                    var assets = Directory.EnumerateFiles(forbiddenAsset, "*", SearchOption.AllDirectories);
                    assets = assets.Select(asset => asset.Replace("\\", "/"));
                    forbiddenAssets.AddRange(assets);
                }
                else
                    forbiddenAssets.Add(forbiddenAsset);
            }
            if (!simulate && forbiddenAssets.Any())
            {
                var forbiddenBuilds = new[] {
                    new AssetBundleBuild
                    {
                        assetBundleName = $"forbidden",
                        assetNames = forbiddenAssets.ToArray()
                    }
                };

                BuildPipeline.BuildAssetBundles(stablePath, forbiddenBuilds, AssetBundleBuildOptions, buildTarget);
            }
            var builds = new AssetBundleBuild[assetBundleDefs.Sum(abd => abd.assetBundles.Length)];
            var buildsIndex = 0;
            for (int defIndex = 0; defIndex < assetBundleDefs.Length; defIndex++)
            {
                var assetBundleDef = assetBundleDefs[defIndex];
                var playerAssemblies = CompilationPipeline.GetAssemblies();
                var assemblyFiles = playerAssemblies.Select(pa => pa.outputPath).ToArray();
                var sourceFiles = playerAssemblies.SelectMany(pa => pa.sourceFiles).ToArray();
                var excludedExtensions = new[] { ".dll", ".cs" };

                var fileCount = 0;

                var logBuilder = new StringBuilder();
                logBuilder.AppendLine("Constructing AssetBundles");
                for (int i = 0; i < assetBundleDef.assetBundles.Length; i++)
                {
                    var def = assetBundleDef.assetBundles[i];
                    buildsIndex++;

                    var explicitAssets = assetBundleDef.assetBundles.Where((ab, abi) => abi != i).SelectMany(ab => ab.assets).Select(asset => AssetDatabase.GetAssetPath(asset)).ToArray();
                    var build = builds[buildsIndex];
                    var assets = new List<string>();
                    logBuilder.AppendLine($"Building bundle: {def.assetBundleName}");
                    if (def.assets.OfType<SceneAsset>().Any())
                        assets.Add(AssetDatabase.GetAssetPath(def.assets.OfType<SceneAsset>().First()));
                    else
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
                    build.assetNames = assets.Where(asset => !forbiddenAssets.Contains(asset)).ToArray();
                    build.assetBundleName = def.assetBundleName;
                    builds[buildsIndex] = build;
                    fileCount += build.assetNames.Length;
                    foreach (var asset in build.assetNames)
                        logBuilder.AppendLine(asset);
                }
                logBuilder.AppendLine($"Constructed {builds.Length} AssetBundleBuilds with {fileCount} files.");

                Debug.Log(logBuilder.ToString());
            }

            if (!simulate)
            {
                BuildPipeline.BuildAssetBundles(stablePath, builds, AssetBundleBuildOptions, buildTarget);

                for (int defIndex = 0; defIndex < assetBundleDefs.Length; defIndex++)
                {
                    var assetBundleDef = assetBundleDefs[defIndex];
                    var bundleNames = assetBundleDef.assetBundles.Select(ab => ab.assetBundleName).ToArray();
                    foreach (var outputPath in assetBundleDef.StagingPaths.Select(path => path.Resolve(pipeline, this)))
                    {
                        
                        foreach (string dirPath in Directory.GetDirectories(stablePath, "*", SearchOption.AllDirectories))
                            Directory.CreateDirectory(dirPath.Replace(stablePath, outputPath));

                        foreach (string filePath in Directory.GetFiles(stablePath, "*", SearchOption.AllDirectories))
                        {
                            var fileName = Path.GetFileName(filePath);
                            if (!bundleNames.Any(bundleName => filePath.Contains(bundleName))) continue;

                            string destFileName = filePath.Replace(stablePath, outputPath);
                            Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
                            File.Copy(filePath, destFileName, true);
                        }
                    }
                }
            }

        }
    }
}
