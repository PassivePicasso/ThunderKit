using System.Linq;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Windows;
using UnityEditor;
using UnityEngine;
#if !UNITY_6000_3_OR_NEWER
using UnityToolbarExtender;
#else
using UnityEditor.Toolbars;
#endif
using static UnityEditor.EditorGUI;
using static UnityEditor.EditorGUILayout;

namespace ThunderKit.Core.Pipelines
{
#if !UNITY_6000_3_OR_NEWER
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
            var pipelineNames = settings.QuickAccessPipelines?
                                        .Where(p => p)
                                        .Select(p => p.name)
                                        .ToArray()
                             ?? System.Array.Empty<string>();

            var manifestNames = settings.QuickAccessManifests?
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
#else
    /// <summary>
    /// Unity 6.3+ implementation using the native Extensible Toolbar API
    /// </summary>
    [InitializeOnLoad]
    public static class PipelineToolbar
    {
        private const int MinDisplayWidth = 20;

        private static ThunderKitSettings _settings;
        private static Texture2D _pipelineIcon;
        private static Texture2D _manifestIcon;

        private static Pipeline _lastPipeline;
        private static Manifest _lastManifest;
        private static int _lastPipelineCount;
        private static int _lastManifestCount;
        private static bool _initialized;

        static PipelineToolbar()
        {
            _initialized = false;
            EditorApplication.delayCall += OnDelayedInit;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnDelayedInit()
        {
            // Defer initialization until Unity has fully loaded assets after domain reload
            _initialized = true;
            _settings = null; // Force re-fetch of settings
            _lastPipeline = Settings.SelectedPipeline;
            _lastManifest = Settings.SelectedManifest;
            _lastPipelineCount = Settings.QuickAccessPipelines?.Length ?? 0;
            _lastManifestCount = Settings.QuickAccessManifests?.Length ?? 0;
            RefreshAllElements();
        }

        private static ThunderKitSettings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
                return _settings;
            }
        }

        private static Texture2D PipelineIcon
        {
            get
            {
                if (_pipelineIcon == null)
                    _pipelineIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Constants.Icons.PipelineIconPath);
                return _pipelineIcon;
            }
        }

        private static Texture2D ManifestIcon
        {
            get
            {
                if (_manifestIcon == null)
                    _manifestIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Constants.Icons.ManifestIconPath);
                return _manifestIcon;
            }
        }

        private static void OnEditorUpdate()
        {
            // Don't run update checks until delayed init has completed
            if (!_initialized) return;

            var currentPipeline = Settings.SelectedPipeline;
            var currentManifest = Settings.SelectedManifest;
            var currentPipelineCount = Settings.QuickAccessPipelines?.Length ?? 0;
            var currentManifestCount = Settings.QuickAccessManifests?.Length ?? 0;

            bool needsRefresh = false;

            if (_lastPipeline != currentPipeline)
            {
                _lastPipeline = currentPipeline;
                needsRefresh = true;
            }

            if (_lastManifest != currentManifest)
            {
                _lastManifest = currentManifest;
                needsRefresh = true;
            }

            if (_lastPipelineCount != currentPipelineCount)
            {
                _lastPipelineCount = currentPipelineCount;
                needsRefresh = true;
            }

            if (_lastManifestCount != currentManifestCount)
            {
                _lastManifestCount = currentManifestCount;
                needsRefresh = true;
            }

            if (needsRefresh)
            {
                RefreshAllElements();
            }
        }

        private static string PadToMinWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new string(' ', MinDisplayWidth);
            if (text.Length >= MinDisplayWidth)
                return text;
            return text.PadRight(MinDisplayWidth);
        }

        [MainToolbarElement("ThunderKit/Pipeline", defaultDockPosition = MainToolbarDockPosition.Right, defaultDockIndex = 100)]
        public static MainToolbarElement PipelineDropdown()
        {
            var pipelineName = Settings.SelectedPipeline != null ? Settings.SelectedPipeline.name : "None";
            var content = new MainToolbarContent(PadToMinWidth(pipelineName), PipelineIcon, "Select a pipeline from Quick Access");

            return new MainToolbarDropdown(content, ShowPipelineMenu);
        }

        [MainToolbarElement("ThunderKit/Manifest", defaultDockPosition = MainToolbarDockPosition.Right, defaultDockIndex = 101)]
        public static MainToolbarElement ManifestDropdown()
        {
            var manifestName = Settings.SelectedManifest != null ? Settings.SelectedManifest.name : "None";
            var content = new MainToolbarContent(PadToMinWidth(manifestName), ManifestIcon, "Select a manifest from Quick Access");

            return new MainToolbarDropdown(content, ShowManifestMenu);
        }

        [MainToolbarElement("ThunderKit/Execute", defaultDockPosition = MainToolbarDockPosition.Right, defaultDockIndex = 102)]
        public static MainToolbarElement ExecuteButton()
        {
            var content = new MainToolbarContent("Execute", "Execute the selected pipeline with the selected manifest");

            return new MainToolbarButton(content, ExecutePipeline)
            {
                enabled = Settings.SelectedPipeline != null
            };
        }

        [MainToolbarElement("ThunderKit/Log", defaultDockPosition = MainToolbarDockPosition.Right, defaultDockIndex = 103)]
        public static MainToolbarElement LogButton()
        {
            var content = new MainToolbarContent("Log", "View the most recent log for the selected pipeline");

            var hasLog = Settings.SelectedPipeline != null &&
                         PipelineLog.PipelineLogs.Any(pl => pl.pipeline == Settings.SelectedPipeline);

            return new MainToolbarButton(content, ShowPipelineLog)
            {
                enabled = hasLog
            };
        }

        private static void ShowPipelineMenu(Rect buttonRect)
        {
            var menu = new GenericMenu();
            var pipelines = Settings.QuickAccessPipelines?.Where(p => p).ToArray() ?? System.Array.Empty<Pipeline>();

            // Add None option
            var noneSelected = Settings.SelectedPipeline == null;
            menu.AddItem(new GUIContent("None"), noneSelected, () =>
            {
                Settings.SelectedPipeline = null;
                RefreshAllElements();
            });

            if (pipelines.Length > 0)
            {
                menu.AddSeparator("");
            }

            foreach (var pipeline in pipelines)
            {
                var isSelected = Settings.SelectedPipeline == pipeline;
                menu.AddItem(new GUIContent(pipeline.name), isSelected, () =>
                {
                    Settings.SelectedPipeline = pipeline;
                    RefreshAllElements();
                });
            }

            menu.DropDown(buttonRect);
        }

        private static void ShowManifestMenu(Rect buttonRect)
        {
            var menu = new GenericMenu();
            var manifests = Settings.QuickAccessManifests?.Where(m => m).ToArray() ?? System.Array.Empty<Manifest>();

            // Add None option
            var noneSelected = Settings.SelectedManifest == null;
            menu.AddItem(new GUIContent("None"), noneSelected, () =>
            {
                Settings.SelectedManifest = null;
                MainToolbar.Refresh("ThunderKit/Manifest");
            });

            if (manifests.Length > 0)
            {
                menu.AddSeparator("");
            }

            foreach (var manifest in manifests)
            {
                var isSelected = Settings.SelectedManifest == manifest;
                menu.AddItem(new GUIContent(manifest.name), isSelected, () =>
                {
                    Settings.SelectedManifest = manifest;
                    MainToolbar.Refresh("ThunderKit/Manifest");
                });
            }

            menu.DropDown(buttonRect);
        }

        private static void RefreshAllElements()
        {
            MainToolbar.Refresh("ThunderKit/Pipeline");
            MainToolbar.Refresh("ThunderKit/Manifest");
            MainToolbar.Refresh("ThunderKit/Execute");
            MainToolbar.Refresh("ThunderKit/Log");
        }

        private static void ExecutePipeline()
        {
            var pipeline = Settings.SelectedPipeline;
            var manifest = Settings.SelectedManifest;

            if (pipeline != null)
            {
                _ = Pipeline.RunPipelineWithManifest(pipeline, manifest);
            }
        }

        private static void ShowPipelineLog()
        {
            var pipeline = Settings.SelectedPipeline;
            if (pipeline == null) return;

            var pipelineLog = PipelineLog.PipelineLogs
                .Where(pl => pl.pipeline == pipeline)
                .OrderByDescending(log => log.CreatedDate)
                .FirstOrDefault();

            if (pipelineLog != null)
            {
                PipelineLogWindow.ShowLog(pipelineLog);
            }
        }
    }
#endif
}