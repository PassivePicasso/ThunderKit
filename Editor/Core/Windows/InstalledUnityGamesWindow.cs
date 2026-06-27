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
        enum Backend { Unknown, Mono, Il2Cpp }
        // None: no Addressables. Json: catalog.json (importable). Binary: catalog.bin (not yet supported).
        enum Catalog { None, Json, Binary }
        // NotApplicable: not an IL2CPP game. Missing: global-metadata.dat absent.
        // Unreadable: present but encrypted/obfuscated/unrecognised header. Readable:
        // valid header, so the game's types can be recovered into editor stub assemblies.
        enum Il2CppMetadata { NotApplicable, Missing, Unreadable, Readable }
        // Supported: ThunderKit's standard Mono path. Experimental: an IL2CPP game whose
        // types are recoverable, so authoring against them is possible but unproven.
        // Unsupported: a blocking caveat prevents targeting it at all.
        enum Compatibility { Supported, Experimental, Unsupported }

        class GameInfo
        {
            public string Name;
            public string Version;     // full player version, e.g. "2021.3.33f1", or "unknown"
            public string Path;
            public bool Matches;       // major.minor.patch matches the running Editor
            public Backend Backend;
            public Catalog Catalog;
            public Il2CppMetadata Metadata;
            public Compatibility Compatibility;
            public string Caveats;     // newline-joined notes (tooltip), or null
        }

        const string UnsupportedVersionCaveat = "Unity version not supported by ThunderKit.";
        const string Il2CppExperimentalCaveat = "IL2CPP backend (experimental): the game's types are recovered from readable IL2CPP metadata for authoring.";
        const string Il2CppMetadataMissingCaveat = "IL2CPP metadata (global-metadata.dat) not found; the game's types cannot be recovered.";
        const string Il2CppMetadataObfuscatedCaveat = "IL2CPP metadata is encrypted or obfuscated; the game's types cannot be recovered.";
        const string BinaryCatalogCaveat = "Binary Addressables catalog is not supported by ThunderKit (JSON only).";

        // global-metadata.dat begins with this magic when it is neither encrypted nor obfuscated.
        const uint Il2CppMetadataMagic = 0xFAB11BAF;

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
            window.minSize = new Vector2(760, 300);
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
                        var backend = DetectBackend(gameDir);
                        var catalog = DetectCatalog(gameDir);
                        var metadata = DetectIl2CppMetadata(gameDir, backend);
                        string caveats;
                        var compatibility = Classify(version, backend, catalog, metadata, out caveats);
                        found.Add(new GameInfo
                        {
                            Name = string.IsNullOrEmpty(name) ? installdir : name,
                            Version = version,
                            Path = gameDir,
                            Matches = version != "unknown" && trimmed == editorVersion,
                            Backend = backend,
                            Catalog = catalog,
                            Metadata = metadata,
                            Compatibility = compatibility,
                            Caveats = caveats,
                        });
                    }
                }

                // Targetable games (supported/experimental) first, then Editor matches,
                // then alphabetical. Unsupported games sink to the bottom.
                games = found
                    .OrderBy(g => g.Compatibility == Compatibility.Unsupported ? 1 : 0)
                    .ThenByDescending(g => g.Matches)
                    .ThenBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                status = string.Format("{0} Unity game(s): {1} match this Editor, {2} supported, {3} experimental IL2CPP target(s), {4} unsupported.",
                    games.Count,
                    games.Count(g => g.Matches),
                    games.Count(g => g.Compatibility == Compatibility.Supported),
                    games.Count(g => g.Compatibility == Compatibility.Experimental),
                    games.Count(g => g.Compatibility == Compatibility.Unsupported));
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
                GUILayout.Label("Backend", EditorStyles.boldLabel, GUILayout.Width(60));
                GUILayout.Label("Addressables", EditorStyles.boldLabel, GUILayout.Width(90));
                GUILayout.Label("Target", EditorStyles.boldLabel, GUILayout.Width(95));
                GUILayout.Label("", GUILayout.Width(60));
            }

            var matchTint = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.40f, 0.22f)
                : new Color(0.74f, 0.92f, 0.74f);
            var experimentalTint = EditorGUIUtility.isProSkin
                ? new Color(0.45f, 0.38f, 0.16f)
                : new Color(0.97f, 0.90f, 0.66f);
            var unsupportedTint = EditorGUIUtility.isProSkin
                ? new Color(0.45f, 0.20f, 0.20f)
                : new Color(0.95f, 0.78f, 0.78f);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var game in games)
            {
                var row = EditorGUILayout.BeginHorizontal();
                if (Event.current.type == EventType.Repaint)
                {
                    // Unsupported (red) takes precedence over an experimental IL2CPP
                    // target (amber), which in turn takes precedence over an Editor
                    // version match (green).
                    if (game.Compatibility == Compatibility.Unsupported)
                        EditorGUI.DrawRect(row, unsupportedTint);
                    else if (game.Compatibility == Compatibility.Experimental)
                        EditorGUI.DrawRect(row, experimentalTint);
                    else if (game.Matches)
                        EditorGUI.DrawRect(row, matchTint);
                }

                var tooltip = game.Caveats;
                var nameStyle = game.Matches ? EditorStyles.boldLabel : EditorStyles.label;
                GUILayout.Label(new GUIContent(game.Name, tooltip), nameStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label(new GUIContent(game.Version, tooltip), nameStyle, GUILayout.Width(110));

                var backendText = BackendLabel(game.Backend);
                GUILayout.Label(new GUIContent(backendText, tooltip), nameStyle, GUILayout.Width(60));

                var catalogText = CatalogLabel(game.Catalog);
                var catalogTip = game.Catalog == Catalog.Binary ? BinaryCatalogCaveat : null;
                GUILayout.Label(new GUIContent(catalogText, catalogTip), nameStyle, GUILayout.Width(90));

                GUILayout.Label(new GUIContent(CompatibilityLabel(game.Compatibility), tooltip), nameStyle, GUILayout.Width(95));

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

        // IL2CPP games ship a GameAssembly native module at the game root and an
        // il2cpp_data folder inside *_Data. Mono games keep their managed assemblies
        // in *_Data/Managed. When neither signature is present we report Unknown
        // rather than guessing.
        static Backend DetectBackend(string gameDir)
        {
            if (File.Exists(Path.Combine(gameDir, "GameAssembly.dll"))
             || File.Exists(Path.Combine(gameDir, "GameAssembly.so"))
             || File.Exists(Path.Combine(gameDir, "GameAssembly.dylib")))
                return Backend.Il2Cpp;

            string[] dataDirs = SafeGetDirectories(gameDir, "*_Data");
            foreach (var data in dataDirs)
                if (Directory.Exists(Path.Combine(data, "il2cpp_data")))
                    return Backend.Il2Cpp;

            foreach (var data in dataDirs)
            {
                var managed = Path.Combine(data, "Managed");
                if (Directory.Exists(managed) && SafeGetFiles(managed, "*.dll").Length > 0)
                    return Backend.Mono;
            }

            return Backend.Unknown;
        }

        // Addressables content lives in *_Data/StreamingAssets/aa. A catalog.bin is
        // produced by the newer binary catalog format; catalog.json by the classic
        // (and only currently importable) JSON format. Hash-suffixed names such as
        // catalog_1234.json are also matched.
        static Catalog DetectCatalog(string gameDir)
        {
            foreach (var data in SafeGetDirectories(gameDir, "*_Data"))
            {
                var aa = Path.Combine(Path.Combine(data, "StreamingAssets"), "aa");
                if (!Directory.Exists(aa))
                    continue;
                if (SafeGetFiles(aa, "catalog*.json").Length > 0)
                    return Catalog.Json;
                if (SafeGetFiles(aa, "catalog*.bin").Length > 0)
                    return Catalog.Binary;
            }
            return Catalog.None;
        }

        static string BackendLabel(Backend backend)
        {
            switch (backend)
            {
                case Backend.Mono: return "Mono";
                case Backend.Il2Cpp: return "IL2CPP";
                default: return "?";
            }
        }

        static string CatalogLabel(Catalog catalog)
        {
            switch (catalog)
            {
                case Catalog.Json: return "JSON";
                case Catalog.Binary: return "Binary";
                default: return "—";
            }
        }

        static string CompatibilityLabel(Compatibility compatibility)
        {
            switch (compatibility)
            {
                case Compatibility.Supported: return "Supported";
                case Compatibility.Experimental: return "Experimental";
                default: return "Unsupported";
            }
        }

        // Determines whether an IL2CPP game's type metadata can be recovered into
        // editor stub assemblies. global-metadata.dat lives under il2cpp_data/Metadata;
        // a valid magic plus a plausible sanity version means it is unencrypted and
        // dumpable. A mismatched header indicates XOR/obfuscated or decoy metadata.
        static Il2CppMetadata DetectIl2CppMetadata(string gameDir, Backend backend)
        {
            if (backend != Backend.Il2Cpp)
                return Il2CppMetadata.NotApplicable;

            string metadataPath = null;
            foreach (var data in SafeGetDirectories(gameDir, "*_Data"))
            {
                var candidate = Path.Combine(Path.Combine(Path.Combine(data, "il2cpp_data"), "Metadata"), "global-metadata.dat");
                if (File.Exists(candidate))
                {
                    metadataPath = candidate;
                    break;
                }
            }
            if (metadataPath == null)
                return Il2CppMetadata.Missing;

            try
            {
                using (var fs = File.OpenRead(metadataPath))
                {
                    var header = new byte[8];
                    if (fs.Read(header, 0, header.Length) < header.Length)
                        return Il2CppMetadata.Unreadable;
                    var magic = BitConverter.ToUInt32(header, 0);
                    var sanityVersion = BitConverter.ToInt32(header, 4);
                    // Known IL2CPP metadata versions span roughly 16..31 across the
                    // Unity 5.x -> 6000 eras; a correct magic with a version outside
                    // that range is treated as suspect rather than recoverable.
                    if (magic == Il2CppMetadataMagic && sanityVersion >= 16 && sanityVersion <= 31)
                        return Il2CppMetadata.Readable;
                    return Il2CppMetadata.Unreadable;
                }
            }
            catch
            {
                return Il2CppMetadata.Unreadable;
            }
        }

        // Classifies a game into a single compatibility tier and collects the notes
        // (used as the row tooltip). A blocking caveat forces Unsupported; a recoverable
        // IL2CPP game is Experimental; everything else is Supported. caveats receives the
        // newline-joined notes, or null when there are none.
        static Compatibility Classify(string version, Backend backend, Catalog catalog, Il2CppMetadata metadata, out string caveats)
        {
            var notes = new List<string>();
            var blocking = false;
            var experimental = false;

            if (!IsSupported(version))
            {
                notes.Add(UnsupportedVersionCaveat);
                blocking = true;
            }
            if (catalog == Catalog.Binary)
            {
                notes.Add(BinaryCatalogCaveat);
                blocking = true;
            }
            if (backend == Backend.Il2Cpp)
            {
                switch (metadata)
                {
                    case Il2CppMetadata.Readable:
                        notes.Add(Il2CppExperimentalCaveat);
                        experimental = true;
                        break;
                    case Il2CppMetadata.Missing:
                        notes.Add(Il2CppMetadataMissingCaveat);
                        blocking = true;
                        break;
                    default:
                        notes.Add(Il2CppMetadataObfuscatedCaveat);
                        blocking = true;
                        break;
                }
            }

            caveats = notes.Count == 0 ? null : string.Join("\n", notes);
            if (blocking)
                return Compatibility.Unsupported;
            return experimental ? Compatibility.Experimental : Compatibility.Supported;
        }

        static string[] SafeGetDirectories(string dir, string pattern)
        {
            try { return Directory.GetDirectories(dir, pattern); }
            catch { return new string[0]; }
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
