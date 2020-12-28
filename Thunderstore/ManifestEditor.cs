#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEngine;
using EGL = UnityEditor.EditorGUILayout;
using EGU = UnityEditor.EditorGUIUtility;
using GL = UnityEngine.GUILayout;

namespace PassivePicasso.ThunderKit.Thunderstore
{
    [CustomEditor(typeof(Manifest))]
    public class ManifestEditor : UnityEditor.Editor
    {
        private const string ROS_Temp = "ros_temp";
        readonly static string TempDir = Path.Combine(Directory.GetCurrentDirectory(), ROS_Temp);

        PackageSearchSuggest suggestor;

        private string dependenciesPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Dependencies");

        private SerializedProperty authorField, versionNumberField, websiteUrlField, descriptionField, dependenciesField,
                                   redistributablesField, patchersField, pluginsField, monomodField, readmeField, iconField;

        /// <summary>
        /// False array element indicates an active installation
        /// True array element indicates a completed installation
        /// a null array indicates no installations being processed
        /// </summary>
        private bool[] activeInstallations;
        private void OnEnable()
        {
            authorField = serializedObject.FindProperty(nameof(Manifest.author));
            versionNumberField = serializedObject.FindProperty(nameof(Manifest.version_number));
            websiteUrlField = serializedObject.FindProperty(nameof(Manifest.website_url));
            descriptionField = serializedObject.FindProperty(nameof(Manifest.description));
            redistributablesField = serializedObject.FindProperty(nameof(Manifest.redistributables));
            dependenciesField = serializedObject.FindProperty(nameof(Manifest.dependencies));
            patchersField = serializedObject.FindProperty(nameof(Manifest.patchers));
            pluginsField = serializedObject.FindProperty(nameof(Manifest.plugins));
            monomodField = serializedObject.FindProperty(nameof(Manifest.monomod));
            readmeField = serializedObject.FindProperty(nameof(Manifest.readme));
            iconField = serializedObject.FindProperty(nameof(Manifest.icon));
            suggestor = CreateInstance<PackageSearchSuggest>();
            suggestor.Evaluate = EvaluateSuggestion;
            suggestor.OnSuggestionGUI = RenderSuggestion;
        }
        private void OnDisable()
        {
            DestroyImmediate(suggestor);
        }

        private bool RenderSuggestion(int arg1, Package package)
        {
            var manifest = target as Manifest;

            if (manifest.dependencies.Contains(package.latest.full_name))
                return false;

            if (GL.Button(package.name))
            {
                var dependencySlot = dependenciesField.GetArrayElementAtIndex(dependenciesField.arraySize++);
                dependencySlot.stringValue = package.latest.full_name;
                dependencySlot.serializedObject.SetIsDifferentCacheDirty();
                dependencySlot.serializedObject.ApplyModifiedProperties();
                suggestor.Cleanup();
                return true;
            }

            return false;
        }

        private IEnumerable<Package> EvaluateSuggestion(string searchString) => ThunderLoad.LookupPackage(searchString);

        public override void OnInspectorGUI()
        {
            var manifest = target as Manifest;
            serializedObject.Update();

            if (manifest.dependencies == null)
                manifest.dependencies = new List<string>();

            AddField(iconField);
            AddField(authorField);
            AddField(versionNumberField);
            AddField(websiteUrlField);
            AddField(descriptionField);
            AddField(readmeField);
            AddField(redistributablesField);
            AddField(patchersField);
            AddField(pluginsField);
            AddField(monomodField);

            serializedObject.SetIsDifferentCacheDirty();
            serializedObject.ApplyModifiedProperties();

            var depCount = manifest.dependencies.Count;
            var rect = EGL.GetControlRect(true, (manifest.dependencies.Count + 1) * EGU.singleLineHeight * 1.5f);

            GUI.Box(rect, "Manifest Dependencies");
            var boxRect = rect;
            if (Event.current.type == EventType.DragUpdated && rect.Contains(Event.current.mousePosition) && DragAndDrop.objectReferences.OfType<Manifest>().Any())
            {
                //Debug.Log("Dragging Manifests");
                foreach (var dependence in DragAndDrop.objectReferences.OfType<Manifest>())
                {
                    if (manifest.dependencies.Any(dp => dp.StartsWith($"{dependence.author}-{dependence.name}")))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.None;
                        break;
                    }
                    else
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                }
            }

            if (Event.current.type == EventType.DragPerform && DragAndDrop.objectReferences.OfType<Manifest>().Any())
            {
                //Debug.Log("Dropping Manifests");
                foreach (var dependence in DragAndDrop.objectReferences.OfType<Manifest>())
                {
                    string dependency = $"{dependence.author}-{dependence.name}-{dependence.version_number}";
                    if (manifest.dependencies.Any(dp => dp.StartsWith($"{dependence.author}-{dependence.name}")))
                        manifest.dependencies.RemoveAll(dp => dp.StartsWith($"{dependence.author}-{dependence.name}"));

                    manifest.dependencies.Add(dependency);
                    DragAndDrop.AcceptDrag();
                }

                return;
            }


            for (int i = 0; i < manifest.dependencies.Count; i++)
            {
                var dependencySlot = dependenciesField.GetArrayElementAtIndex(i);

                var size = new Vector2(boxRect.size.x - EGU.singleLineHeight * 2, EGU.singleLineHeight);
                size = new Vector2(size.x * 1.5f, size.y * 1.5f);
                rect = new Rect(rect.position + Vector2.up * EGU.singleLineHeight, size);

                GUI.Label(rect, dependencySlot.stringValue);

                var buttonSize = new Vector2(EGU.singleLineHeight * 2, EGU.singleLineHeight);
                var buttonPosition = new Rect(boxRect.position.x + boxRect.size.x - buttonSize.x,
                                              rect.position.y, 25, EGU.singleLineHeight);
                if (GUI.Button(buttonPosition, "x"))
                {
                    var dependencyPath = Path.Combine(dependenciesPath, dependencySlot.stringValue);

                    if (Directory.Exists(dependencyPath)) Directory.Delete(dependencyPath, true);

                    dependenciesField.DeleteArrayElementAtIndex(i);

                    dependenciesField.serializedObject.SetIsDifferentCacheDirty();

                    dependenciesField.serializedObject.ApplyModifiedProperties();

                    AssetDatabase.Refresh();
                }
            }

            if (manifest.dependencies.Any())
                if (GL.Button("Download & Install Dependencies"))
                {
                    IEnumerable<Package> RecurseDeps(IEnumerable<string> dependencies)
                    {
                        var deps = dependencies.SelectMany(dep => ThunderLoad.LookupPackage(dep));
                        var subDeps = deps.SelectMany(idep => idep.latest.dependencies).Distinct();

                        if (subDeps.Any())
                            return deps.Union(RecurseDeps(subDeps));

                        return deps;
                    }

                    var distinctResults = RecurseDeps(manifest.dependencies).GroupBy(dep => dep.latest.full_name).Select(g => g.First());
                    var packages = distinctResults.Where(dep => !dep.latest.full_name.Contains("BepInEx")).ToList();
                    activeInstallations = new bool[packages.Count];

                    if (!Directory.Exists(TempDir)) Directory.CreateDirectory(TempDir);

                    async void Install(Package package, int i)
                    {
                        string filePath = await ThunderLoad.DownloadPackageAsync(package, Path.Combine(TempDir, GetZipFileName(package)));

                        var dependencyPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Dependencies", package.latest.full_name);

                        if (Directory.Exists(dependencyPath)) Directory.Delete(dependencyPath, true);

                        if (File.Exists($"{dependencyPath}.meta")) File.Delete($"{dependencyPath}.meta");

                        Directory.CreateDirectory(dependencyPath);

                        using (var fileStream = File.OpenRead(filePath))
                        using (var archive = new ZipArchive(fileStream))
                            archive.ExtractToDirectory(Path.Combine(dependencyPath));
                    }
                    for (int i = 0; i < packages.Count; i++)
                        Install(packages[i], i);
                }

            if (activeInstallations != null && activeInstallations.All(b => b))
            {
                AssetDatabase.Refresh();
                activeInstallations = null;
                if (Directory.Exists(TempDir))
                    Directory.Delete(TempDir, true);
            }

            var alignment = GUI.skin.button.alignment;
            var margin = GUI.skin.button.margin;
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            GUI.skin.button.margin = new RectOffset(1, 1, 1, 0);
            suggestor.OnSuggestGUI("Dependency Search");
            GUI.skin.button.alignment = alignment;
            GUI.skin.button.margin = margin;
        }

        private void AddField(SerializedProperty property)
        {
            var rect = EGL.GetControlRect(true, EGU.singleLineHeight);
            EditorGUI.PropertyField(rect, property, property.isArray);
            if (property.isExpanded)
                EGL.GetControlRect(true, EGU.singleLineHeight * (property.arraySize + 1));
        }

        private static string GetZipFileName(Package package) => GetZipFileName(package.latest.full_name);
        private static string GetZipFileName(string package)
        {
            return $"{package}.zip";
        }
    }
}
#endif