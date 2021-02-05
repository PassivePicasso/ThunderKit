using System.IO;
using System.Linq;
using ThunderKit.Common.Package;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Paths;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor]
    public class StageUnityPackages : PipelineJob
    {
        public bool remapFileIds = true;
        public override void Execute(Pipeline pipeline)
        {
            var unityPackageData = pipeline.Manifest.Data.OfType<UnityPackages>().ToArray();
            var remappableAssets = unityPackageData
                .SelectMany(upd => upd.unityPackages)
                .SelectMany(up => up.AssetFiles)
                .Select(AssetDatabase.GetAssetPath)
                .SelectMany(path =>
                {
                    if (AssetDatabase.IsValidFolder(path))
                        return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
                    else
                        return Enumerable.Repeat(path, 1);
                })
                .Select(path => (path, asset: AssetDatabase.LoadAssetAtPath<Object>(path)))
                .SelectMany(map =>
                {
                    var (path, asset) = map;
                    if (asset is GameObject goAsset)
                        return goAsset.GetComponentsInChildren<MonoBehaviour>()
                                         .Select(mb => (path: path, monoScript: MonoScript.FromMonoBehaviour(mb)));

                    if (asset is ScriptableObject soAsset)
                        return Enumerable.Repeat((path: path, monoScript: MonoScript.FromScriptableObject(soAsset)), 1);

                    return Enumerable.Empty<(string path, MonoScript monoScript)>();
                })
                .Select(map =>
                {
                    var type = map.monoScript.GetClass();
                    var fileId = FileIdUtil.Compute(type);
                    var libraryGuid = PackageHelper.GetAssemblyHash(type.Assembly.Location);
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(map.monoScript, out string scriptGuid, out long scriptId);

                    return (Path: map.path,
                            ScriptReference: $"{{fileID: {scriptId}, guid: {scriptGuid}, type: 3}}",
                            AssemblyReference: $"{{fileID: {fileId}, guid: {libraryGuid}, type: 3}}");
                })
                .ToArray();

            if (remapFileIds)
            {
                AssetDatabase.StopAssetEditing();
                foreach (var map in remappableAssets)
                {
                    var fileText = File.ReadAllText(map.Path);
                    fileText = fileText.Replace(map.ScriptReference, map.AssemblyReference);
                    File.WriteAllText(map.Path, fileText);
                }
                AssetDatabase.StartAssetEditing();
                AssetDatabase.Refresh();
            }

            foreach (var unityPackageDatum in unityPackageData)
            {
                foreach (var outputPath in unityPackageDatum.StagingPaths.Select(path => path.Resolve(pipeline, this)))
                {
                    if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

                    foreach (var unityPackage in unityPackageDatum.unityPackages)
                        UnityPackage.Export(unityPackage, outputPath);
                }
            }

            if (remapFileIds)
            {
                AssetDatabase.StopAssetEditing();
                foreach (var map in remappableAssets)
                {
                    var fileText = File.ReadAllText(map.Path);
                    fileText = fileText.Replace(map.AssemblyReference, map.ScriptReference);
                    File.WriteAllText(map.Path, fileText);
                }
                AssetDatabase.StartAssetEditing();
                AssetDatabase.Refresh();
            }
        }
    }
}