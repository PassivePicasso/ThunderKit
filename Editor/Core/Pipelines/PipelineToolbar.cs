using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using ThunderKit.Core.Manifests;
using ThunderKit.Common;
using ThunderKit.Core.Windows;
using ThunderKit.Core.Data;

namespace ThunderKit.Core.Pipelines
{
    [InitializeOnLoad]
    public class PipelineToolbar
    {
        public struct PipelineToolbarPrefs
        {
            public Pipeline selectedPipeline;
            public Manifest selectedManifest;
        }

        static PipelineToolbarPrefs pipelineToolbarPrefs;
        private static readonly string PipelineToolbarPrefsKey = "ThunderKit_Pipeline_ToolbarPrefs";
        private static Texture2D pipelineIcon, manifestIcon;
        private static GUIStyle manifestStyle;
        private static GUIStyle pipelineStyle;

        static PipelineToolbar()
        {
            manifestIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Constants.Icons.ManifestIconPath);
            pipelineIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Constants.Icons.PipelineIconPath);

            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

            if (EditorPrefs.HasKey(PipelineToolbarPrefsKey))
            {
                var json = EditorPrefs.GetString(PipelineToolbarPrefsKey);
                pipelineToolbarPrefs = JsonUtility.FromJson<PipelineToolbarPrefs>(json);
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

            var pipelines = AssetDatabase.FindAssets($"t:{nameof(Pipeline)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Pipeline>)
                .Where(pipeline => pipeline.QuickAccess)
                .Select((pipeline, index) => (pipeline, index))
                .ToArray();
            var pipelineNames = pipelines.Select(pair => pair.pipeline.name).ToArray();
            var manifests = AssetDatabase.FindAssets($"t:{nameof(Manifest)}", Constants.FindAssetsFolders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Manifest>)
                .Where(manifest => manifest.QuickAccess)
                .Select((manifest, index) => (manifest, index))
                .ToArray();
            var manifestsNames = manifests.Select(pair => pair.manifest.name).ToArray();

            var selectedPipelineIndex = pipelines.FirstOrDefault(pair => pair.pipeline == pipelineToolbarPrefs.selectedPipeline).index;
            var selectedManifestIndex = manifests.FirstOrDefault(pair => pair.manifest == pipelineToolbarPrefs.selectedManifest).index;

            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(140)))
            {
                GUILayout.Space(1);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(string.Empty, pipelineStyle);
                    selectedPipelineIndex = EditorGUILayout.Popup(selectedPipelineIndex, pipelineNames, popupStyle);
                }
            }
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(140)))
            {
                GUILayout.Space(1);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(string.Empty, manifestStyle);
                    selectedManifestIndex = EditorGUILayout.Popup(selectedManifestIndex, manifestsNames, popupStyle);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                pipelineToolbarPrefs.selectedPipeline = pipelines[selectedPipelineIndex].pipeline;
                pipelineToolbarPrefs.selectedManifest = manifests[selectedManifestIndex].manifest;
                EditorPrefs.SetString(PipelineToolbarPrefsKey, JsonUtility.ToJson(pipelineToolbarPrefs));
            }
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Space(2);
                using (new EditorGUILayout.HorizontalScope())
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

                    if (GUILayout.Button("Log"))
                    {
                        var pipelineLog = AssetDatabase.FindAssets($"t:{nameof(PipelineLog)}")
                                                        .Select(AssetDatabase.GUIDToAssetPath)
                                                        .Where(ap => ap.Contains(pipeline.name))
                                                        .Select(AssetDatabase.LoadAssetAtPath<PipelineLog>)
                                                        .OrderByDescending(log => log.CreatedDate)
                                                        .First();

                        PipelineLogWindow.ShowLog(pipelineLog);
                    }
                }
            }
            GUILayout.FlexibleSpace();
            GUI.skin = origSkin;

        }
    }
}