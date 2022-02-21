using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using ThunderKit.Core.Manifests;
using ThunderKit.Common;
using ThunderKit.Core.Windows;
using static UnityEditor.EditorGUI;
using static UnityEditor.EditorGUILayout;
using System.IO;
using System;

namespace ThunderKit.Core.Pipelines
{
    [InitializeOnLoad]
    public class PipelineToolbar
    {
        public struct PipelineToolbarPrefs
        {
            public int pipelineInstanceId;
            public int manifestInstanceId;
            public Pipeline selectedPipeline
            {
                get => AssetDatabase.FindAssets($"t:{nameof(Pipeline)}", Constants.FindAllFolders)
                                 .Select(AssetDatabase.GUIDToAssetPath)
                                 .Select(AssetDatabase.LoadAssetAtPath<Pipeline>)
                                 .Where(obj => obj.QuickAccess)
                                 .First(obj => obj.GetInstanceID() == pipelineToolbarPrefs.pipelineInstanceId);
                set => pipelineInstanceId = value?.GetInstanceID() ?? 0;
            }
            public Manifest selectedManifest
            {
                get => AssetDatabase.FindAssets($"t:{nameof(Manifest)}", Constants.FindAllFolders)
                                 .Select(AssetDatabase.GUIDToAssetPath)
                                 .Select(AssetDatabase.LoadAssetAtPath<Manifest>)
                                 .Where(obj => obj.QuickAccess)
                                 .First(obj => obj.GetInstanceID() == pipelineToolbarPrefs.manifestInstanceId);
                set => manifestInstanceId = value?.GetInstanceID() ?? 0;
            }
        }

        private static readonly string PrefPath = "ProjectSettings/ThunderKit/PipelineToolbarPrefs.json";
        static PipelineToolbarPrefs pipelineToolbarPrefs;
        private static Texture2D pipelineIcon, manifestIcon;
        private static GUIStyle manifestStyle;
        private static GUIStyle pipelineStyle;

        static PipelineToolbar()
        {
            manifestIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Constants.Icons.ManifestIconPath);
            pipelineIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Constants.Icons.PipelineIconPath);

            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

            if (File.Exists(PrefPath))
            {
                var json = File.ReadAllText(PrefPath);
                pipelineToolbarPrefs = JsonUtility.FromJson<PipelineToolbarPrefs>(json);
            }
            else
            {
                pipelineToolbarPrefs = default;

                pipelineToolbarPrefs.selectedPipeline = AssetDatabase.FindAssets($"t:{nameof(Pipeline)}", Constants.FindAllFolders)
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<Pipeline>)
                    .Where(pipeline => pipeline.QuickAccess)
                    .FirstOrDefault();
                pipelineToolbarPrefs.selectedManifest = AssetDatabase.FindAssets($"t:{nameof(Manifest)}", Constants.FindAllFolders)
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<Manifest>)
                    .Where(manifest => manifest.QuickAccess)
                    .FirstOrDefault();
                Directory.CreateDirectory(Path.GetDirectoryName(PrefPath));
                File.WriteAllText(PrefPath, JsonUtility.ToJson(pipelineToolbarPrefs));
            }

            manifestStyle = new GUIStyle()
            {
                normal = new GUIStyleState
                {
                    background = manifestIcon
                },
                margin = new RectOffset(0, 0, 1, 0),
                overflow = new RectOffset(0, 7, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                fixedWidth = 13,
                fixedHeight = 20
            };
            pipelineStyle = new GUIStyle(manifestStyle)
            {
                normal = new GUIStyleState
                {
                    background = pipelineIcon
                }
            };
        }
        static void OnToolbarGUI()
        {
            GUISkin origSkin = GUI.skin;
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            var popupStyle = GUI.skin.GetStyle("popup");
            popupStyle.margin.top = 2;
            popupStyle.padding.left = 4;

            var pipelines = AssetDatabase.FindAssets($"t:{nameof(Pipeline)}", Constants.FindAllFolders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Pipeline>)
                .Where(pipeline => pipeline.QuickAccess)
                .Select((pipeline, index) => (pipeline, index))
                .ToArray();
            var pipelineNames = pipelines.Select(pair => pair.pipeline.name).ToArray();
            var manifests = AssetDatabase.FindAssets($"t:{nameof(Manifest)}", Constants.FindAllFolders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Manifest>)
                .Where(manifest => manifest.QuickAccess)
                .Select((manifest, index) => (manifest, index))
                .ToArray();
            var manifestsNames = manifests.Select(pair => pair.manifest.name).ToArray();

            var selectedPipelineIndex = pipelines.FirstOrDefault(pair => pair.pipeline == pipelineToolbarPrefs.selectedPipeline).index;
            var selectedManifestIndex = manifests.FirstOrDefault(pair => pair.manifest == pipelineToolbarPrefs.selectedManifest).index;

            BeginChangeCheck();
            using (new VerticalScope(GUILayout.Width(140)))
            {
                GUILayout.Space(1);
                using (new HorizontalScope())
                {
                    GUILayout.Label(string.Empty, pipelineStyle);
                    selectedPipelineIndex = Popup(selectedPipelineIndex, pipelineNames, popupStyle);
                }
            }
            using (new VerticalScope(GUILayout.Width(140)))
            {
                GUILayout.Space(1);
                using (new HorizontalScope())
                {
                    GUILayout.Label(string.Empty, manifestStyle);
                    selectedManifestIndex = Popup(selectedManifestIndex, manifestsNames, popupStyle);
                }
            }
            if (EndChangeCheck())
            {
                if (selectedPipelineIndex > -1 && selectedPipelineIndex < pipelines.Length)
                    pipelineToolbarPrefs.selectedPipeline = pipelines[selectedPipelineIndex].pipeline;
                if (selectedManifestIndex > -1 && selectedManifestIndex < manifests.Length)
                    pipelineToolbarPrefs.selectedManifest = manifests[selectedManifestIndex].manifest;

                File.WriteAllText(PrefPath, JsonUtility.ToJson(pipelineToolbarPrefs));
            }
            using (new VerticalScope())
            {
                GUILayout.Space(2);
                using (new HorizontalScope())
                {
                    try
                    {
                        var pipeline = pipelineToolbarPrefs.selectedPipeline;

                        if (GUILayout.Button("Execute"))
                        {
                            var manifest = pipelineToolbarPrefs.selectedManifest;
                            // pipeline.manifest is the correct field to use, stop checking every time.
                            // pipeline.manifest is the manifest that is assigned to the pipeline containing this job via the editor
                            var originalManifest = pipeline.manifest;
                            try
                            {
                                if (manifest)
                                {
                                    pipeline.manifest = manifest;
                                    _ = pipeline.Execute();
                                    pipeline.manifest = originalManifest;
                                }
                                else
                                    _ = pipeline.Execute();
                            }
                            finally
                            {
                                pipeline.manifest = originalManifest;
                                EditorUtility.SetDirty(pipeline);
                            }

                        }
                        var pipelineLog = AssetDatabase.FindAssets($"t:{nameof(PipelineLog)}")
                                                       .Select(AssetDatabase.GUIDToAssetPath)
                                                       .Where(ap => ap.Contains(pipeline.name))
                                                       .Select(AssetDatabase.LoadAssetAtPath<PipelineLog>)
                                                       .OrderByDescending(log => log.CreatedDate)
                                                       .FirstOrDefault();
                        using (new DisabledScope(!pipelineLog))
                            if (GUILayout.Button("Log"))
                            {
                                if (pipelineLog)
                                    PipelineLogWindow.ShowLog(pipelineLog);
                            }

                    }
                    finally { }
                }
            }
            GUILayout.FlexibleSpace();
            GUI.skin = origSkin;

        }
    }
}