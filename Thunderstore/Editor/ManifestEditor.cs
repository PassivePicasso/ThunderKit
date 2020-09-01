#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using EGL = UnityEditor.EditorGUILayout;
using EGU = UnityEditor.EditorGUIUtility;
using GL = UnityEngine.GUILayout;

namespace PassivePicasso.ThunderKit.Thunderstore.Editor
{
    [CustomEditor(typeof(Manifest))]
    public class ManifestEditor : UnityEditor.Editor
    {
        private const string ROS_Temp = "ros_temp";
        readonly static string TempDir = Path.Combine(Directory.GetCurrentDirectory(), ROS_Temp);
        SearchField searchField;
        string searchString;
        List<Package> searchResults;

        private volatile bool searching = false, installing = false;

        private string dependenciesPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Dependencies");

        private SerializedProperty versionNumberField, websiteUrlField, descriptionField, dependencies;

        /// <summary>
        /// False array element indicates an active installation
        /// True array element indicates a completed installation
        /// a null array indicates no installations being processed
        /// </summary>
        private bool[] activeInstallations;
        private void OnEnable()
        {
            versionNumberField = serializedObject.FindProperty("version_number");
            websiteUrlField = serializedObject.FindProperty("website_url");
            descriptionField = serializedObject.FindProperty("description");
            dependencies = serializedObject.FindProperty("dependencies");
        }

        public override void OnInspectorGUI()
        {
            if (searchField == null)
                searchField = new SearchField { autoSetFocusOnFindCommand = true };

            var manifest = target as Manifest;
            serializedObject.Update();

            if (manifest.dependencies == null)
                manifest.dependencies = new List<string>();

            AddField(versionNumberField);
            AddField(websiteUrlField);
            AddField(descriptionField);

            serializedObject.SetIsDifferentCacheDirty();
            serializedObject.ApplyModifiedProperties();

            var rect = EGL.GetControlRect(true, EGU.singleLineHeight);
            GUI.Label(rect, "Manifest Dependencies");

            var depCount = manifest.dependencies.Count;
            rect = EGL.GetControlRect(true, (manifest.dependencies.Count + 1) * EGU.singleLineHeight * 1.5f);

            GUI.Box(rect, "Manifest Dependencies");
            var boxRect = rect;

            for (int i = 0; i < manifest.dependencies.Count; i++)
            {
                var dependencySlot = dependencies.GetArrayElementAtIndex(i);

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

                    dependencies.DeleteArrayElementAtIndex(i);

                    dependencies.serializedObject.SetIsDifferentCacheDirty();

                    dependencies.serializedObject.ApplyModifiedProperties();

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
                    installing = true;
                    for (int i = 0; i < packages.Count; i++)
                        Install(packages[i], i);
                }

            if (activeInstallations != null && activeInstallations.All(b => b))
            {
                AssetDatabase.Refresh();
                activeInstallations = null;
                installing = false;
                Directory.Delete(TempDir, true);
            }

            rect = EGL.GetControlRect(true, EGU.singleLineHeight);

            var labelRect = new Rect(rect.position,
                            new Vector2(EGU.labelWidth, EGU.singleLineHeight));

            var fieldRect = new Rect(rect.position + Vector2.right * EGU.labelWidth,
                            rect.size - Vector2.right * EGU.labelWidth);

            GUI.Label(labelRect, "Dependency Search");


            searchString = searchField.OnGUI(fieldRect, searchString);

            if (Event.current.isKey && Event.current.keyCode == KeyCode.Return && searchField.HasFocus())
            {
                searchResults = ThunderLoad.LookupPackage(searchString).ToList();
                searching = true;
            }
            else
            {
                if (string.IsNullOrEmpty(searchString)) searchResults = null;

                if (searchResults != null)
                {
                    EGL.BeginVertical();

                    foreach (var result in searchResults)
                    {
                        if (manifest.dependencies.Contains(result.latest.full_name))
                            continue;
                        if (GL.Button(result.name))
                        {
                            var dependencySlot = dependencies.GetArrayElementAtIndex(dependencies.arraySize++);
                            dependencySlot.stringValue = result.latest.full_name;
                            dependencySlot.serializedObject.SetIsDifferentCacheDirty();
                            dependencySlot.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    searching = false;
                    EGL.EndVertical();
                }
            }
        }

        public override bool RequiresConstantRepaint() => searching || installing;

        private void AddField(SerializedProperty property)
        {
            var rect = EGL.GetControlRect(true, EGU.singleLineHeight);
            EditorGUI.PropertyField(rect, property);
        }

        private static string GetZipFileName(Package package) => GetZipFileName(package.latest.full_name);
        private static string GetZipFileName(string package)
        {
            return $"{package}.zip";
        }
    }
}
#endif