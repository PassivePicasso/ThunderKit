using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Data;
using ThunderKit.Core.Editor;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace ThunderKit.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), RequiresManifestDatumType(typeof(AssetBundleDefinitions))]
    public class StageAssetBundles : PipelineJob
    {
        [EnumFlag]
        public BuildAssetBundleOptions AssetBundleBuildOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        public bool recurseDirectories;
        public bool simulate;

        [PathReferenceResolver]
        public string BundleArtifactPath = "<AssetBundleStaging>";
        public override void Execute(Pipeline pipeline)
        {
            var excludedExtensions = new[] { ".dll", ".cs", ".meta" };

            AssetDatabase.SaveAssets();

            var assetBundleDefs = pipeline.Datums.OfType<AssetBundleDefinitions>().ToArray();
            var bundleArtifactPath = BundleArtifactPath.Resolve(pipeline, this);
            Directory.CreateDirectory(bundleArtifactPath);

            var builds = new AssetBundleBuild[assetBundleDefs.Sum(abd => abd.assetBundles.Length)];
            var buildsIndex = 0;
            for (int defIndex = 0; defIndex < assetBundleDefs.Length; defIndex++)
            {
                var assetBundleDef = assetBundleDefs[defIndex];
                var playerAssemblies = CompilationPipeline.GetAssemblies();
                var assemblyFiles = playerAssemblies.Select(pa => pa.outputPath).ToArray();
                var sourceFiles = playerAssemblies.SelectMany(pa => pa.sourceFiles).ToArray();

                var fileCount = 0;

                var logBuilder = new StringBuilder();
                logBuilder.AppendLine("Constructing AssetBundles");
                for (int i = 0; i < assetBundleDef.assetBundles.Length; i++)
                {
                    var def = assetBundleDef.assetBundles[i];

                    var explicitAssets = assetBundleDef.assetBundles.Where((ab, abi) => abi != i).SelectMany(ab => ab.assets).Select(asset => AssetDatabase.GetAssetPath(asset)).ToArray();
                    var build = builds[buildsIndex];

                    var assets = new List<string>();
                    
                    logBuilder.AppendLine($"Building bundle: {def.assetBundleName}");

                    if (def.assets.OfType<SceneAsset>().Any()) assets.Add(AssetDatabase.GetAssetPath(def.assets.OfType<SceneAsset>().First()));
                    else
                        foreach (var asset in def.assets)
                        {
                            var assetPath = AssetDatabase.GetAssetPath(asset);

                            if (AssetDatabase.IsValidFolder(assetPath))
                                assets.AddRange(Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories)
                                      .SelectMany(ap => AssetDatabase.GetDependencies(ap).Union(new[] { ap })));

                            else if (asset is UnityPackage up)
                            {
                                if (up.exportPackageOptions.HasFlag(ExportPackageOptions.Recurse))
                                    foreach (var upAsset in up.AssetFiles)
                                    {
                                        var path = AssetDatabase.GetAssetPath(upAsset);
                                        if (AssetDatabase.IsValidFolder(path))
                                            assets.AddRange(Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                                                  .SelectMany(ap => AssetDatabase.GetDependencies(ap).Union(new[] { ap })));
                                        else
                                            assets.Add(path);
                                    }
                            }
                            else
                                assets.AddRange(AssetDatabase.GetDependencies(assetPath)
                                    .Where(ap => AssetDatabase.GetMainAssetTypeAtPath(ap) != typeof(UnityPackage))
                                    .Union(new[] { assetPath }));
                        }

                    build.assetNames = assets
                        .Select(ap => ap.Replace("\\", "/"))
                        //.Where(dap => !explicitDownstreamAssets.Contains(dap))
                        .Where(dap => !explicitAssets.Contains(dap))
                        .Where(dap => !excludedExtensions.Contains(Path.GetExtension(dap)))
                        .Where(ap => !sourceFiles.Contains(ap))
                        .Where(ap => !assemblyFiles.Contains(ap))
                        .Where(path => !AssetDatabase.IsValidFolder(path))
                        .Distinct()
                        .ToArray();
                    build.assetBundleName = def.assetBundleName;
                    builds[buildsIndex] = build;
                    buildsIndex++;

                    fileCount += build.assetNames.Length;
                    foreach (var asset in build.assetNames)
                        logBuilder.AppendLine(asset);
                }
                logBuilder.AppendLine($"Constructed {builds.Length} AssetBundleBuilds with {fileCount} files.");

                Debug.Log(logBuilder.ToString());
            }

            if (!simulate)
            {
                var allBuilds = builds.ToArray();
                BuildPipeline.BuildAssetBundles(bundleArtifactPath, allBuilds, AssetBundleBuildOptions, buildTarget);
                for (pipeline.ManifestIndex = 0; pipeline.ManifestIndex < pipeline.manifests.Length; pipeline.ManifestIndex++)
                {
                    var manifest = pipeline.Manifest;
                    foreach(var assetBundleDef in manifest.Data.OfType<AssetBundleDefinitions>())
                    {
                        var bundleNames = assetBundleDef.assetBundles.Select(ab => ab.assetBundleName).ToArray();
                        foreach (var outputPath in assetBundleDef.StagingPaths.Select(path => path.Resolve(pipeline, this)))
                        {
                            foreach (string dirPath in Directory.GetDirectories(bundleArtifactPath, "*", SearchOption.AllDirectories))
                                Directory.CreateDirectory(dirPath.Replace(bundleArtifactPath, outputPath));

                            foreach (string filePath in Directory.GetFiles(bundleArtifactPath, "*", SearchOption.AllDirectories))
                            {
                                bool found = false;
                                foreach (var bundleName in bundleNames)
                                {
                                    if (filePath.ToLower().Contains(bundleName.ToLower()))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found) continue;
                                string destFileName = filePath.Replace(bundleArtifactPath, outputPath);
                                Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
                                File.Copy(filePath, destFileName, true);
                            }

                            File.Copy(Path.Combine(bundleArtifactPath, $"{Path.GetFileName(bundleArtifactPath)}.manifest"),
                                      Path.Combine(outputPath, $"{manifest.Identity.Name}.manifest"),
                                      true);
                        }
                    }
                }
                pipeline.ManifestIndex = -1;
            }
        }
    }
}
