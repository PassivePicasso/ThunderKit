using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using ThunderKit.Core.Manifests;
using ThunderKit.Common;
using ThunderKit.Core.Windows;

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

        private const string SkinPath = "Packages/com.passivepicasso.thunderkit/Editor/Skins/ThunderKitSkin.guiskin";
        static PipelineToolbarPrefs pipelineToolbarPrefs;
        private static GUISkin skin;
        private static readonly string PipelineToolbarPrefsKey = "ThunderKit_Pipeline_ToolbarPrefs";
        static PipelineToolbar()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

            if (EditorPrefs.HasKey(PipelineToolbarPrefsKey))
            {
                var json = EditorPrefs.GetString(PipelineToolbarPrefsKey);
                pipelineToolbarPrefs = JsonUtility.FromJson<PipelineToolbarPrefs>(json);
            }
            skin = AssetDatabase.LoadAssetAtPath<GUISkin>(SkinPath);
        }


        static void OnToolbarGUI()
        {
            GUISkin origSkin = GUI.skin;
            GUI.skin = skin;

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

            GUILayout.Space(90);
            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Space(2);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(string.Empty, "PipelineIcon");
                    selectedPipelineIndex = EditorGUILayout.Popup(selectedPipelineIndex, pipelineNames, "PipelinePopup");
                }
            }
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Space(2);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(string.Empty, "ManifestIcon");
                    selectedManifestIndex = EditorGUILayout.Popup(selectedManifestIndex, manifestsNames, "ManifestPopup");
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

                    if (GUILayout.Button("Log", GUILayout.Height(15)))
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
            GUILayout.Space(90);
            GUI.skin = origSkin;

        }
    }
}