using ThunderKit.Thunderstore.Manifests;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;
using GL = UnityEngine.GUILayout;
using static ThunderKit.Thunderstore.Constants;

namespace ThunderKit.Thunderstore.Editor
{
    using Manifest = Core.Manifests.Manifest;

    [CustomPropertyDrawer(typeof(DependencyList))]
    public class DependenciesDrawer : PropertyDrawer
    {
        PackageSearchSuggest suggestor = new PackageSearchSuggest
        {
            Evaluate = EvaluateSuggestion,
        };
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            rect = new Rect(rect.x, rect.y, rect.width + 2, rect.height);

            var thunderManifest = property.serializedObject.targetObject as ThunderstoreManifest;
            var manifest = AssetDatabase.LoadAssetAtPath<Manifest>(AssetDatabase.GetAssetPath(thunderManifest));

            bool RenderSuggestion(int arg1, Package package)
            {
                if (thunderManifest.dependencies.Contains(package.latest.full_name))
                    return false;

                if (GL.Button(package.name))
                {
                    thunderManifest.dependencies.Add(package.latest.full_name);
                    property.serializedObject.SetIsDifferentCacheDirty();
                    property.serializedObject.ApplyModifiedProperties();
                    suggestor.Cleanup();
                    
                    if (!Directory.Exists(TempDir)) Directory.CreateDirectory(TempDir);

                    var packages = RecurseDependencies(thunderManifest.dependencies)
                        .GroupBy(dep => dep.latest.full_name).Select(g => g.First()).ToArray();

                    foreach (var pack in packages)
                        ThunderstoreAPI.DownloadPackage(package, Path.Combine(TempDir, GetZipFileName(package)));

                    return true;
                }

                return false;
            }
            suggestor.OnSuggestionGUI = RenderSuggestion;

            if (Event.current.type == EventType.DragUpdated
             && rect.Contains(Event.current.mousePosition)
             && DragAndDrop.objectReferences.OfType<Manifest>().Any())
            {
                //Debug.Log("Dragging Manifests");
                var canDrop = false;
                var manifests = DragAndDrop.objectReferences.OfType<Manifest>();

                foreach (var droppedManifest in manifests)
                    foreach (var dependence in droppedManifest.Data.OfType<ThunderstoreManifest>())
                        if (!dependence.dependencies.Any(dp => dp.StartsWith($"{thunderManifest.author}-{manifest.name}")))
                        {
                            canDrop = true;
                            break;
                        }

                DragAndDrop.visualMode = canDrop ? DragAndDropVisualMode.Link : DragAndDropVisualMode.None;
            }

            if (Event.current.type == EventType.DragPerform && DragAndDrop.objectReferences.OfType<Manifest>().Any())
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
                    }
            }

            var baseRect = rect;
            GUI.Box(rect, "Dependencies");

            for (int i = 0; i < thunderManifest.dependencies.Count; i++)
            {
                var depName = thunderManifest.dependencies[i];

                var size = new Vector2(rect.size.x - singleLineHeight * 2, singleLineHeight);
                size = new Vector2(size.x * 1.5f, size.y * 1.5f);
                baseRect = new Rect(baseRect.position + (Vector2.up * singleLineHeight), size);

                GUI.Label(new Rect(baseRect.position + (Vector2.right * 4), baseRect.size), depName);

                var buttonSize = new Vector2(singleLineHeight * 2, singleLineHeight);
                var buttonPosition = new Rect(rect.position.x + rect.size.x - buttonSize.x,
                                              baseRect.position.y + 1, 25, singleLineHeight - 1);
                if (GUI.Button(buttonPosition, "x"))
                {
                    var dependencyPath = Path.Combine(Packages, depName);

                    if (Directory.Exists(dependencyPath)) Directory.Delete(dependencyPath, true);

                    var listed = thunderManifest.dependencies.ToList();
                    listed.RemoveAt(i);
                    thunderManifest.dependencies = new DependencyList(listed);

                    property.serializedObject.SetIsDifferentCacheDirty();

                    property.serializedObject.ApplyModifiedProperties();

                    AssetDatabase.Refresh();
                }
            }

            var suggestRect = rect;
            suggestRect.y = rect.yMax - ((standardVerticalSpacing * 2) + singleLineHeight + 1);
            suggestRect.height = singleLineHeight;
            suggestRect.x += 4;
            suggestRect.width -= 8;

            suggestor.OnSuggestGUI(ref suggestRect, "Dependency Search");
        }
        private static string GetZipFileName(string package) => $"{package}.zip";
        private static string GetZipFileName(Package package) => GetZipFileName(package.latest.full_name);
        IEnumerable<Package> RecurseDependencies(IEnumerable<string> dependencies)
        {
            var deps = dependencies.SelectMany(dep => ThunderstoreAPI.LookupPackage(dep));
            var subDeps = deps.SelectMany(idep => idep.latest.dependencies).Distinct();

            if (subDeps.Any())
                return deps.Union(RecurseDependencies(subDeps));

            return deps;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var manifest = property.serializedObject.targetObject as ThunderstoreManifest;
            var sizeCounter = manifest.dependencies.Count + 2;
            return sizeCounter * (standardVerticalSpacing + singleLineHeight);
        }
        private static IEnumerable<Package> EvaluateSuggestion(string searchString) => ThunderstoreAPI.LookupPackage(searchString);
    }
}