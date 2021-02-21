using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ThunderKit.Common;
using ThunderKit.Common.Package;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests;
using ThunderKit.Integrations.Thunderstore.Manifests;
using UnityEditor;
using UnityEngine;
using static ThunderKit.Integrations.Thunderstore.CreateThunderstoreManifest;
using static UnityEditor.EditorGUIUtility;
using static UnityEngine.GUILayout;

namespace ThunderKit.Integrations.Thunderstore.Editor
{
    [CustomEditor(typeof(ThunderstoreManifest), true)]
    public class ThunderstoreManifestEditor : UnityEditor.Editor
    {
        PackageSearchSuggest suggestor = new PackageSearchSuggest
        {
            Evaluate = EvaluateSuggestion,
        };
        private Rect dragDropRect;

        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, "m_Script", nameof(ThunderstoreManifest.dependencies));

            var property = serializedObject.FindProperty(nameof(ThunderstoreManifest.dependencies));

            var thunderManifest = serializedObject.targetObject as ThunderstoreManifest;
            var manifest = AssetDatabase.LoadAssetAtPath<Manifest>(AssetDatabase.GetAssetPath(thunderManifest));
            if (thunderManifest)
            {

                using (new VerticalScope(GUI.skin.box))
                {
                    Label("Dependencies");
                    for (int i = 0; i < thunderManifest.dependencies.Count; i++)
                    {
                        var depName = thunderManifest.dependencies[i];

                        Label(depName);

                        var bp = GUILayoutUtility.GetLastRect();
                        bp = new Rect(bp.width + 4, bp.y + 1, 13, bp.height - 2);
                        if (Event.current.type == EventType.Repaint)
                        {
                            GUI.skin.box.Draw(bp, new GUIContent(""), 0);
                            GUIContent content = new GUIContent("x");
                            var contentSize = GUIStyle.none.CalcSize(content);
                            GUIStyle.none.Draw(new Rect(bp.x + 3, bp.y - 1, bp.width, bp.height), content, 0);
                        }
                        if (Event.current.type == EventType.MouseUp && bp.Contains(Event.current.mousePosition))
                        {
                            var dependencyPath = Path.Combine(Constants.Packages, depName);

                            if (Directory.Exists(dependencyPath)) Directory.Delete(dependencyPath, true);

                            var listed = thunderManifest.dependencies.ToList();
                            listed.RemoveAt(i);
                            thunderManifest.dependencies = new DependencyList(listed);

                            property.serializedObject.SetIsDifferentCacheDirty();

                            property.serializedObject.ApplyModifiedProperties();

                            AssetDatabase.Refresh();
                        }
                    }

                    var suggestRect = GUILayoutUtility.GetRect(currentViewWidth, singleLineHeight);
                    suggestRect.x++;
                    suggestRect.width -= 4;

                    suggestor.OnSuggestionGUI = RenderSuggestion;
                    suggestor.OnSuggestGUI(suggestRect, "Dependency Search");
                    Space(2);
                }
            }

            switch (Event.current.type)
            {
                case EventType.Repaint:
                    dragDropRect = GUILayoutUtility.GetLastRect();
                    break;
                case EventType.DragUpdated:
                    if (!dragDropRect.Contains(Event.current.mousePosition))
                        break;

                    if (DragAndDrop.objectReferences.OfType<Manifest>().Any())
                    {
                        var canDrop = false;
                        var manifests = DragAndDrop.objectReferences.OfType<Manifest>().ToArray();

                        foreach (var droppedManifest in manifests)
                            foreach (var depThunderManifest in droppedManifest.Data.OfType<ThunderstoreManifest>())
                            {
                                string thisGuid = $"{thunderManifest.author}-{manifest.name}";
                                if (!depThunderManifest.dependencies.Any(dp => dp.StartsWith(thisGuid))
                                 && !thisGuid.StartsWith($"{depThunderManifest.author}-{droppedManifest.name}"))
                                {
                                    canDrop = true;
                                    break;
                                }
                                if (canDrop) break;
                            }
                        if (canDrop)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                            Event.current.Use();
                            return;
                        }
                    }
                    if (DragAndDrop.paths.Any(path => Path.GetExtension(path).Equals(".zip")))
                    {
                        var canDrop = false;
                        foreach (var path in DragAndDrop.paths.Where(path => Path.GetExtension(path).Equals(".zip")))
                        {
                            using (var archive = new ZipArchive(File.OpenRead(path)))
                            {
                                foreach (var entry in archive.Entries)
                                {
                                    if (!"manifest.json".Equals(Path.GetFileName(entry.FullName), System.StringComparison.OrdinalIgnoreCase)) continue;
                                    canDrop = true;
                                    break;
                                }
                            }
                        }

                        if (canDrop)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                            Event.current.Use();
                            return;
                        }
                    }
                    break;
                case EventType.DragPerform:
                    if (DragAndDrop.objectReferences.OfType<Manifest>().Any())
                    {
                        //Debug.Log("Dropping Manifests");
                        var manifests = DragAndDrop.objectReferences.OfType<Manifest>();
                        foreach (var droppedManifest in manifests)
                            foreach (var dependence in droppedManifest.Data.OfType<ThunderstoreManifest>())
                            {
                                string dependency = $"{dependence.author}-{droppedManifest.name}-{dependence.versionNumber}";
                                if (thunderManifest.dependencies.Any(dp => dp.StartsWith($"{dependence.author}-{droppedManifest.name}")))
                                    thunderManifest.dependencies.RemoveAll(dp => dp.StartsWith($"{dependence.author}-{droppedManifest.name}"));

                                if (thunderManifest.dependencies == null || !thunderManifest.dependencies.Any())
                                    thunderManifest.dependencies = new DependencyList();

                                thunderManifest.dependencies.Add(dependency);
                                property.serializedObject.SetIsDifferentCacheDirty();
                                property.serializedObject.ApplyModifiedProperties();
                                DragAndDrop.AcceptDrag();
                                Event.current.Use();
                                return;
                            }
                    }
                    if (DragAndDrop.paths.Any(path => Path.GetExtension(path).Equals(".zip")))
                    {
                        bool refresh = false;
                        foreach (var path in DragAndDrop.paths.Where(path => Path.GetExtension(path).Equals(".zip")))
                        {

                            using (var archive = new ZipArchive(File.OpenRead(path)))
                            {
                                var entry = archive.Entries.FirstOrDefault(e => "manifest.json".Equals(Path.GetFileName(e.FullName), System.StringComparison.OrdinalIgnoreCase));
                                if (entry == null) continue;

                                var archiveName = Path.GetFileNameWithoutExtension(path);
                                var outputDir = Path.Combine("Packages", archiveName);
                                refresh = true;

                                Directory.CreateDirectory(outputDir);
                                archive.ExtractToDirectory(outputDir);

                                foreach(var assemblyPath in Directory.EnumerateFiles(outputDir, "*.dll", SearchOption.AllDirectories))
                                {
                                    PackageHelper.WriteAssemblyMetaData(assemblyPath, $"{assemblyPath}.meta");
                                }

                                using (var reader = new StreamReader(entry.Open()))
                                {
                                    var stubManifest = JsonUtility.FromJson<ThunderstoreManifestStub>(reader.ReadToEnd());
                                    string author = stubManifest.author ?? archiveName.Substring(0, archiveName.IndexOf('-'));
                                    string guid = $"{author}-{stubManifest.name}-{stubManifest.version_number}";
                                    Debug.Log($"Added {guid} to {manifest.name} dependencies");
                                    thunderManifest.dependencies.Add(guid);
                                    PackageHelper.GeneratePackageManifest(
                                        stubManifest.name.ToLower(), outputDir,
                                        stubManifest.name, "Thunderstore",
                                        stubManifest.version_number,
                                        stubManifest.description,
                                        ThunderKitSetting.GetOrCreateSettings<ThunderstoreSettings>().ThunderstoreUrl);
                                }
                            }
                        }
                        if (refresh) AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);

                    }
                    break;
            }

            bool RenderSuggestion(int arg1, PackageListing package)
            {
                if (thunderManifest.dependencies.Contains(package.latest.full_name))
                    return false;

                if (Button(package.name))
                {
                    thunderManifest.dependencies.Add(package.latest.full_name);
                    property.serializedObject.SetIsDifferentCacheDirty();
                    property.serializedObject.ApplyModifiedProperties();
                    suggestor.Cleanup();

                    if (!Directory.Exists(Constants.TempDir)) Directory.CreateDirectory(Constants.TempDir);

                    var packages = RecurseDependencies(thunderManifest.dependencies)
                        .GroupBy(dep => dep.latest.full_name).Select(g => g.First()).ToArray();

                    foreach (var pack in packages)
                        ThunderstoreAPI.DownloadPackage(pack, Path.Combine(Constants.TempDir, GetZipFileName(pack)));

                    return true;
                }

                return false;
            }
        }

        private static string GetZipFileName(string package) => $"{package}.zip";
        private static string GetZipFileName(PackageListing package) => GetZipFileName(package.latest.full_name);
        IEnumerable<PackageListing> RecurseDependencies(IEnumerable<string> dependencies)
        {
            var deps = dependencies.SelectMany(dep => ThunderstoreAPI.LookupPackage(dep));
            var subDeps = deps.SelectMany(idep => idep.latest.dependencies).Distinct();

            if (subDeps.Any())
                return deps.Union(RecurseDependencies(subDeps));

            return deps;
        }

        private static IEnumerable<PackageListing> EvaluateSuggestion(string searchString) => ThunderstoreAPI.LookupPackage(searchString);
    }
}