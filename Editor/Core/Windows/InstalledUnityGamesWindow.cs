using AssetsTools.NET.Extra;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ThunderKit.Common;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ThunderKit.Core.Windows
{
    /// <summary>
    /// Scans installed Steam games and lists the Unity games among them along
    /// with the Unity version they were built with. Games whose version matches
    /// the running Editor are highlighted.
    /// </summary>
    public class InstalledUnityGamesWindow : EditorWindow
    {
        class GameInfo
        {
            public string Name;
            public string Version;     // full player version, e.g. "2021.3.33f1", or "unknown"
            public string Path;
            public bool Matches;       // major.minor.patch matches the running Editor
            public bool Supported;     // built with Unity 2018.4 or newer
        }

        const string UnsupportedTooltip = "Unity version not supported by ThunderKit.";

        // Trims a Unity version down to major.minor.patch (drops the fXX suffix).
        static readonly Regex VersionTrim = new Regex(@"(\d{1,4}\.\d+\.\d+)(.*)");
        // Locates a full Unity version string inside a binary asset header.
        static readonly Regex VersionScan = new Regex(@"\d+\.\d+\.\d+[abcfpx]\d+");

        List<GameInfo> games;
        string editorVersionFull;
        string editorVersion;
        string status;
        Vector2 scroll;

        [MenuItem(Constants.ThunderKitMenuRoot + "Installed Unity Games", priority = Constants.ThunderKitMenuPriority)]
        public static void ShowWindow()
        {
            var window = GetWindow<InstalledUnityGamesWindow>();
            window.titleContent = new GUIContent("Unity Games");
            window.minSize = new Vector2(520, 300);
            window.Scan();
            window.Show();
        }

        void OnEnable()
        {
            if (games == null)
                Scan();
        }

        void Scan()
        {
            editorVersionFull = Application.unityVersion;
            editorVersion = VersionTrim.Replace(editorVersionFull, m => m.Groups[1].Value);

            var found = new List<GameInfo>();
            try
            {
                foreach (var libraryRoot in GetLibraryFolders())
                {
                    var steamapps = Path.Combine(libraryRoot, "steamapps");
                    var common = Path.Combine(steamapps, "common");
                    if (!Directory.Exists(steamapps))
                        continue;

                    foreach (var acf in Directory.GetFiles(steamapps, "appmanifest_*.acf"))
                    {
                        string text;
                        try { text = File.ReadAllText(acf); }
                        catch { continue; }

                        var installdir = MatchGroup(text, "\"installdir\"\\s+\"([^\"]+)\"");
                        if (string.IsNullOrEmpty(installdir))
                            continue;

                        // StateFlags bit 4 (StateFullyInstalled) must be set.
                        var stateRaw = MatchGroup(text, "\"StateFlags\"\\s+\"([^\"]+)\"");
                        int state;
                        if (int.TryParse(stateRaw, out state) && (state & 4) == 0)
                            continue;

                        var gameDir = Path.Combine(common, installdir);
                        if (!Directory.Exists(gameDir))
                            continue;

                        string version;
                        if (!TryDetectUnity(gameDir, out version))
                            continue;

                        var name = MatchGroup(text, "\"name\"\\s+\"([^\"]+)\"");
                        var trimmed = VersionTrim.Replace(version, m => m.Groups[1].Value);
                        found.Add(new GameInfo
                        {
                            Name = string.IsNullOrEmpty(name) ? installdir : name,
                            Version = version,
                            Path = gameDir,
                            Matches = version != "unknown" && trimmed == editorVersion,
                            Supported = IsSupported(version),
                        });
                    }
                }

                // Matches first, then alphabetical.
                games = found
                    .OrderByDescending(g => g.Matches)
                    .ThenBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                status = string.Format("{0} Unity game(s) found ({1} matching this Editor, {2} unsupported).",
                    games.Count, games.Count(g => g.Matches), games.Count(g => !g.Supported));
            }
            catch (Exception e)
            {
                games = new List<GameInfo>();
                status = "Scan failed: " + e.Message;
                Debug.LogException(e);
            }
        }

        void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Editor: " + editorVersionFull, EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                    Scan();
            }

            EditorGUILayout.HelpBox(status ?? "", MessageType.None);

            if (games == null || games.Count == 0)
            {
                EditorGUILayout.LabelField("No installed Unity games found.");
                return;
            }

            // Header row
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Game", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Unity Version", EditorStyles.boldLabel, GUILayout.Width(110));
                GUILayout.Label("", GUILayout.Width(60));
            }

            var matchTint = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.40f, 0.22f)
                : new Color(0.74f, 0.92f, 0.74f);
            var unsupportedTint = EditorGUIUtility.isProSkin
                ? new Color(0.45f, 0.20f, 0.20f)
                : new Color(0.95f, 0.78f, 0.78f);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var game in games)
            {
                var row = EditorGUILayout.BeginHorizontal();
                if (Event.current.type == EventType.Repaint)
                {
                    // Unsupported (red) takes precedence over an Editor match (green).
                    if (!game.Supported)
                        EditorGUI.DrawRect(row, unsupportedTint);
                    else if (game.Matches)
                        EditorGUI.DrawRect(row, matchTint);
                }

                var tooltip = game.Supported ? null : UnsupportedTooltip;
                var nameStyle = game.Matches ? EditorStyles.boldLabel : EditorStyles.label;
                GUILayout.Label(new GUIContent(game.Name, tooltip), nameStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label(new GUIContent(game.Version, tooltip), nameStyle, GUILayout.Width(110));
                if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(60)))
                    EditorUtility.RevealInFinder(game.Path);

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        #region Steam discovery

        static IEnumerable<string> GetLibraryFolders()
        {
            var steamPath = FindSteamPath();
            var roots = new List<string>();
            if (!string.IsNullOrEmpty(steamPath))
            {
                roots.Add(steamPath);

                // The remaining libraries are listed in libraryfolders.vdf, which
                // lives under steamapps (modern) or config (older). Both expose a
                // "path" "<value>" pair, so a single scan covers both layouts.
                foreach (var vdf in new[]
                {
                    Path.Combine(Path.Combine(steamPath, "steamapps"), "libraryfolders.vdf"),
                    Path.Combine(Path.Combine(steamPath, "config"), "libraryfolders.vdf"),
                })
                {
                    if (!File.Exists(vdf))
                        continue;
                    string text;
                    try { text = File.ReadAllText(vdf); }
                    catch { continue; }
                    foreach (Match m in Regex.Matches(text, "\"path\"\\s+\"([^\"]+)\""))
                        roots.Add(m.Groups[1].Value.Replace("\\\\", "\\"));
                    break;
                }
            }

            return roots
                .Where(Directory.Exists)
                .Select(p => Path.GetFullPath(p).TrimEnd(Path.DirectorySeparatorChar))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        static string FindSteamPath()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                try
                {
                    var p = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
                    if (!string.IsNullOrEmpty(p) && Directory.Exists(p))
                        return p;
                }
                catch { }
                try
                {
                    var p = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null) as string
                          ?? Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null) as string;
                    if (!string.IsNullOrEmpty(p) && Directory.Exists(p))
                        return p;
                }
                catch { }
                foreach (var candidate in new[] { @"C:\Program Files (x86)\Steam", @"C:\Program Files\Steam" })
                    if (Directory.Exists(candidate))
                        return candidate;
                return null;
            }

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] candidates;
            if (Application.platform == RuntimePlatform.OSXEditor)
                candidates = new[] { Path.Combine(home, "Library/Application Support/Steam") };
            else // Linux
                candidates = new[]
                {
                    Path.Combine(home, ".steam/steam"),
                    Path.Combine(home, ".local/share/Steam"),
                    Path.Combine(home, ".var/app/com.valvesoftware.Steam/data/Steam"),
                };
            foreach (var c in candidates)
                if (Directory.Exists(c))
                    return c;
            return null;
        }

        #endregion

        #region Unity detection

        static bool TryDetectUnity(string gameDir, out string version)
        {
            version = null;

            string[] dataDirs;
            try { dataDirs = Directory.GetDirectories(gameDir, "*_Data"); }
            catch { return false; }

            var unityPlayer = Path.Combine(gameDir, "UnityPlayer.dll");
            var hasUnityPlayer = File.Exists(unityPlayer);
            if (dataDirs.Length == 0 && !hasUnityPlayer)
                return false;

            // Preferred: read the serialized Unity version via AssetsTools.NET,
            // the same approach ThunderKit uses in CheckUnityVersion.
            foreach (var data in dataDirs)
            {
                foreach (var file in new[] { "globalgamemanagers", "data.unity3d" })
                {
                    var path = Path.Combine(data, file);
                    if (!File.Exists(path))
                        continue;
                    try
                    {
                        var am = new AssetsManager();
                        var asset = am.LoadAssetsFile(path, false);
                        var v = asset != null ? asset.file.Metadata.UnityVersion : null;
                        am.UnloadAll(true);
                        if (!string.IsNullOrEmpty(v) && VersionScan.IsMatch(v))
                        {
                            version = v;
                            return true;
                        }
                    }
                    catch { }
                }
            }

            // Fallback: scan asset headers for the version string. Covers old
            // Unity 4/5 layouts that use mainData rather than globalgamemanagers.
            foreach (var data in dataDirs)
            {
                foreach (var file in new[] { "globalgamemanagers", "data.unity3d", "mainData", "resources.assets" })
                {
                    var v = ScanFileForVersion(Path.Combine(data, file));
                    if (v != null)
                    {
                        version = v;
                        return true;
                    }
                }
            }

            // Fallback: file version metadata from UnityPlayer.dll or the executable.
            var dll = ReadDllVersion(unityPlayer);
            if (dll != null) { version = dll; return true; }
            foreach (var exe in SafeGetFiles(gameDir, "*.exe"))
            {
                var ev = ReadDllVersion(exe);
                if (ev != null) { version = ev; return true; }
            }

            // It is a Unity game (has _Data) but the version couldn't be read.
            version = "unknown";
            return dataDirs.Length > 0;
        }

        static string ScanFileForVersion(string path)
        {
            if (!File.Exists(path))
                return null;
            try
            {
                using (var fs = File.OpenRead(path))
                {
                    var buf = new byte[8192];
                    int read = fs.Read(buf, 0, buf.Length);
                    // Version digits/letters are ASCII; non-ASCII bytes become '?'
                    // which never appears inside a match, so ASCII decoding is safe.
                    var text = Encoding.ASCII.GetString(buf, 0, read);
                    var m = VersionScan.Match(text);
                    return m.Success ? m.Value : null;
                }
            }
            catch { return null; }
        }

        static string ReadDllVersion(string path)
        {
            if (!File.Exists(path))
                return null;
            try
            {
                var info = FileVersionInfo.GetVersionInfo(path);
                var pv = info.ProductVersion ?? info.FileVersion;
                if (pv != null) pv = pv.Trim();
                // Some games strip version metadata, leaving "0.0.0.0".
                if (string.IsNullOrEmpty(pv) || pv.StartsWith("0.0"))
                    return null;
                return pv;
            }
            catch { return null; }
        }

        static string[] SafeGetFiles(string dir, string pattern)
        {
            try { return Directory.GetFiles(dir, pattern); }
            catch { return new string[0]; }
        }

        static string MatchGroup(string input, string pattern)
        {
            var m = Regex.Match(input, pattern);
            return m.Success ? m.Groups[1].Value : null;
        }

        // True unless the version is demonstrably older than Unity 2018.4.
        // "unknown" / unparseable versions are treated as supported because we
        // can't prove they're old. Unity's numbering is monotonic across the
        // 4.x/5.x -> 2017+ -> 6000+ eras, so a numeric major.minor compare works.
        static bool IsSupported(string version)
        {
            int major, minor;
            if (!TryParseMajorMinor(version, out major, out minor))
                return true;
            if (major < 2018) return false;
            if (major == 2018 && minor < 4) return false;
            return true;
        }

        static bool TryParseMajorMinor(string version, out int major, out int minor)
        {
            major = 0;
            minor = 0;
            if (string.IsNullOrEmpty(version))
                return false;
            var m = Regex.Match(version, @"^(\d+)\.(\d+)");
            if (!m.Success)
                return false;
            return int.TryParse(m.Groups[1].Value, out major)
                 && int.TryParse(m.Groups[2].Value, out minor);
        }

        #endregion
    }
}
