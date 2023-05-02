using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Windows;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using static UnityEditor.EditorGUI;
using static UnityEditor.EditorGUILayout;

namespace ThunderKit.Core.Pipelines
{
    [InitializeOnLoad]
    public class PipelineToolbar
    {
        private static readonly string DefaultGuid = default(GUID).ToString();
        public struct PipelineToolbarPrefs
        {
            public string pipelineGuid;
            public string manifestGuid;
            private Pipeline _selectedPipeline;
            private Manifest _selectedManifest;

            public Pipeline selectedPipeline
            {
                get
                {
                    if (string.IsNullOrEmpty(pipelineGuid) || pipelineGuid == DefaultGuid)
                        return null;
                    if (_selectedPipeline)
                        return _selectedPipeline;
                    return _selectedPipeline = AssetDatabase.LoadAssetAtPath<Pipeline>(AssetDatabase.GUIDToAssetPath(pipelineGuid));
                }
                set
                {
                    if (value)
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value, out pipelineGuid, out long _);
                    else
                        pipelineGuid = DefaultGuid;

                    _selectedPipeline = value;
                }
            }
            public Manifest selectedManifest
            {
                get
                {
                    if (string.IsNullOrEmpty(manifestGuid) || manifestGuid == DefaultGuid)
                        return null;
                    if (_selectedManifest)
                        return _selectedManifest;
                    return _selectedManifest = AssetDatabase.LoadAssetAtPath<Manifest>(AssetDatabase.GUIDToAssetPath(manifestGuid));
                }

                set
                {
                    if (value)
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value, out manifestGuid, out long _);
                    else
                        manifestGuid = default(GUID).ToString();

                    _selectedManifest = value;
                }
            }
        }

        private static Texture2D pipelineIcon, manifestIcon;
        private static ThunderKitSettings settings;
        private static GUIStyle manifestStyle;
        private static GUIStyle pipelineStyle;

        static PipelineToolbar()
        {
            manifestIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Constants.Icons.ManifestIconPath);
            pipelineIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Constants.Icons.PipelineIconPath);

            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);

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


        static bool manifestContained, pipelineContained;
        private static GUIStyle popupStyle;

        static void OnToolbarGUI()
        {
            if (settings == null)
            {
                settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            }

            GUISkin origSkin = GUI.skin;
            if (popupStyle == null)
            {
                var skin = GUI.skin;
                GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
                popupStyle = new GUIStyle(GUI.skin.GetStyle("popup"));
                popupStyle.margin.top = 2;
                popupStyle.padding.left = 4;
                GUI.skin = skin;
            }
            var pipelineNames = settings.QuickAccessPipelines
                                        .Where(p => p)
                                        .Select(p => p.name)
                                        .ToArray()
                             ?? System.Array.Empty<string>();

            var manifestNames = settings.QuickAccessManifests
                                        .Where(p => p)
                                        .Select(p => p.name)
                                        .ToArray()
                             ?? System.Array.Empty<string>();

            var selectedPipelineName = settings.SelectedPipeline != null ? settings.SelectedPipeline.name : string.Empty;
            var selectedManifestName = settings.SelectedManifest != null ? settings.SelectedManifest.name : string.Empty;

            var selectedPipelineIndex = System.Array.IndexOf(pipelineNames, selectedPipelineName);
            var selectedManifestIndex = System.Array.IndexOf(manifestNames, selectedManifestName);

            BeginChangeCheck();
            selectedPipelineIndex = AdvancedPopup(settings.SelectedPipeline, pipelineNames, selectedPipelineIndex, pipelineStyle);
            selectedManifestIndex = AdvancedPopup(settings.SelectedManifest, manifestNames, selectedManifestIndex, manifestStyle);
            if (EndChangeCheck())
            {
                if (selectedPipelineIndex > -1 && selectedPipelineIndex < settings.QuickAccessPipelines.Length)
                    settings.SelectedPipeline = settings.QuickAccessPipelines[selectedPipelineIndex];

                if (selectedManifestIndex > -1 && selectedManifestIndex < settings.QuickAccessManifests.Length)
                    settings.SelectedManifest = settings.QuickAccessManifests[selectedManifestIndex];
            }
            using (new VerticalScope())
            {
                GUILayout.Space(2);
                using (new HorizontalScope())
                {
                    try
                    {
                        var pipeline = settings.SelectedPipeline;
                        var manifest = settings.SelectedManifest;

                        using (new DisabledScope((!pipeline)))
                            if (GUILayout.Button("Execute"))
                                _ = Pipeline.RunPipelineWithManifest(pipeline, manifest);

                        PipelineLog pipelineLog = null;
                        if (pipeline)
                            pipelineLog = PipelineLog.PipelineLogs
                                                     .Where(pl => pl.pipeline == pipeline)
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


        private static int AdvancedPopup(Object obj, string[] pipelineNames, int selectedPipelineIndex, GUIStyle labelStyle)
        {
            using (new VerticalScope(GUILayout.Width(140)))
            {
                GUILayout.Space(1);
                using (new HorizontalScope())
                {
                    GUILayout.Label(string.Empty, labelStyle);
                    var lastRect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.Link);
                    if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUIUtility.PingObject(obj);
                        Selection.activeObject = obj;
                    }

                    selectedPipelineIndex = Popup(selectedPipelineIndex, pipelineNames, popupStyle);
                }
            }

            return selectedPipelineIndex;
        }
    }
}