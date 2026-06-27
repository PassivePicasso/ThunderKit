using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ThunderKit.Core.Config
{
    /// <summary>
    /// <see cref="Il2CppStubGenerator"/> backed by Cpp2IL
    /// (https://github.com/SamboyCoding/Cpp2IL). Invokes the Cpp2IL executable as an
    /// external process with <c>--output-as dll_default</c> to recover the game's
    /// types into stub assemblies, then returns the ones the project doesn't already
    /// provide (assemblies already loaded in the Editor are dropped so they don't clash
    /// with the real ones).
    ///
    /// Cpp2IL is run out-of-process because its managed library targets modern .NET
    /// and will not reliably load in the Editor's Mono runtime. If no executable is
    /// configured, the matching self-contained build is downloaded automatically from
    /// Cpp2IL's GitHub releases and cached under Library/ThunderKit/Cpp2IL.
    ///
    /// Everything is overridable via EditorPrefs: an explicit executable path
    /// (Tools/ThunderKit/Set Cpp2IL Executable) or the CPP2IL environment variable
    /// take precedence over the download; the pinned version, the full download URL,
    /// and the argument template can each be overridden to absorb CLI/asset drift
    /// between Cpp2IL versions.
    /// </summary>
    [Serializable]
    public class Cpp2IlStubGenerator : Il2CppStubGenerator
    {
        public const string ExecutablePrefKey = "ThunderKit.Cpp2IL.ExecutablePath";
        public const string ArgumentsPrefKey = "ThunderKit.Cpp2IL.Arguments";
        public const string VersionPrefKey = "ThunderKit.Cpp2IL.Version";
        public const string UrlPrefKey = "ThunderKit.Cpp2IL.DownloadUrl";
        const string EnvironmentVariable = "CPP2IL";

        // Pinned to a version whose CLI matches DefaultArguments below. Bump both
        // together (the --output-as plugin CLI arrived in the 2022.1.0-pre line).
        const string DefaultVersion = "2022.1.0-pre-release.21";
        const string ReleaseUrlFormat = "https://github.com/SamboyCoding/Cpp2IL/releases/download/{0}/Cpp2IL-{0}-{1}";
        static readonly string CacheDirectory = Path.Combine("Library", "ThunderKit", "Cpp2IL");

        // {0} = game directory, {1} = output directory. dll_default = stubs with full
        // type/field/attribute metadata and valid, non-throwing method bodies (methods
        // return default values). The Editor instantiates these types during inspector,
        // drag, and Add Component operations, so constructors must not throw - which
        // rules out dll_throw_null (every ctor throws NRE) and dummydll/dll_empty (empty
        // bodies Unity rejects as broken). (Formats verified against Cpp2IL
        // 2022.1.0-pre-release.21 --help / --list-output-formats.)
        const string DefaultArguments = "--game-path \"{0}\" --output-to \"{1}\" --output-as dll_default";

        public override int Priority => 0;

        // Acquisition is deferred to TryGenerate (it may download), so CanGenerate
        // stays cheap and side-effect-free: the platform just has to be supported.
        public override bool CanGenerate(ThunderKitSettings settings)
            => Il2CppUtility.IsIl2Cpp(settings) && PlatformSuffix() != null;

        public override bool TryGenerate(ThunderKitSettings settings, string outputDirectory, out IReadOnlyList<string> stubAssemblyPaths)
        {
            stubAssemblyPaths = Array.Empty<string>();

            var executable = EnsureExecutable();
            if (executable == null)
            {
                Debug.LogWarning("[ThunderKit] Cpp2IL is unavailable. Use Tools/ThunderKit/Acquire Cpp2IL to download it, Set Cpp2IL Executable to point at a local build, or set the CPP2IL environment variable.");
                return false;
            }

            var template = EditorPrefs.GetString(ArgumentsPrefKey, DefaultArguments);
            var arguments = string.Format(template, settings.GamePath, outputDirectory);

            if (!RunCpp2Il(executable, arguments, outputDirectory))
                return false;

            var editorProvided = LoadedAssemblyNames();
            var produced = Directory.EnumerateFiles(outputDirectory, "*.dll", SearchOption.AllDirectories)
                .Where(path => !editorProvided.Contains(Path.GetFileNameWithoutExtension(path)))
                .Distinct()
                .ToList();

            if (produced.Count == 0)
            {
                Debug.LogWarning($"[ThunderKit] Cpp2IL produced no game assemblies under {outputDirectory}.");
                return false;
            }

            Debug.Log($"[ThunderKit] Cpp2IL recovered {produced.Count} game assembl{(produced.Count == 1 ? "y" : "ies")} for {settings.PackageName}.");
            stubAssemblyPaths = produced;
            return true;
        }

        static bool RunCpp2Il(string executable, string arguments, string workingDirectory)
        {
            // Framework-dependent Cpp2IL ships as a .dll that must be launched via dotnet;
            // self-contained builds (Cpp2IL-Win.exe, etc.) are launched directly.
            var isDll = string.Equals(Path.GetExtension(executable), ".dll", StringComparison.OrdinalIgnoreCase);
            var fileName = isDll ? "dotnet" : executable;
            var fullArguments = isDll ? $"\"{executable}\" {arguments}" : arguments;

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = fullArguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var output = new StringBuilder();
            try
            {
                using (var process = new Process { StartInfo = startInfo })
                {
                    process.OutputDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
                    process.ErrorDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        Debug.LogError($"[ThunderKit] Cpp2IL exited with code {process.ExitCode}:\n{output}");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ThunderKit] Failed to run Cpp2IL ({executable}): {e.Message}\n{output}");
                return false;
            }
            return true;
        }

        // Assemblies the Editor/project already provide. Cpp2IL emits the game's full
        // referenced set (BCL, UnityEngine modules, etc.); importing stub copies of those
        // would shadow or clash with the real ones, so anything already loaded is dropped.
        // Everything else - game gameplay assemblies, Unity packages the project lacks,
        // third-party - is kept so the game's Assembly-CSharp reference graph resolves.
        static HashSet<string> LoadedAssemblyNames()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm == null) continue;
                try { names.Add(asm.GetName().Name); } catch { }
            }
            return names;
        }

        // Returns a usable executable, downloading and caching one if necessary.
        static string EnsureExecutable() => ResolveExecutable() ?? Acquire();

        // Resolves an already-available executable without any network access:
        // explicit override, then environment variable, then the download cache.
        static string ResolveExecutable()
        {
            var stored = EditorPrefs.GetString(ExecutablePrefKey, null);
            if (!string.IsNullOrEmpty(stored) && File.Exists(stored))
                return stored;

            var fromEnv = Environment.GetEnvironmentVariable(EnvironmentVariable);
            if (!string.IsNullOrEmpty(fromEnv) && File.Exists(fromEnv))
                return fromEnv;

            var cached = CachedExecutablePath();
            if (cached != null && File.Exists(cached))
                return cached;

            return null;
        }

        // Downloads the self-contained Cpp2IL build for this platform into the cache
        // and returns its path, or null on failure / unsupported platform.
        static string Acquire()
        {
            var suffix = PlatformSuffix();
            if (suffix == null)
            {
                Debug.LogWarning($"[ThunderKit] No prebuilt Cpp2IL is available for {Application.platform}. Set Cpp2IL Executable manually.");
                return null;
            }

            var version = EditorPrefs.GetString(VersionPrefKey, DefaultVersion);
            var destination = CachedExecutablePath();
            if (File.Exists(destination))
                return destination;

            var url = EditorPrefs.GetString(UrlPrefKey, null);
            if (string.IsNullOrEmpty(url))
                url = string.Format(ReleaseUrlFormat, version, suffix);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                EditorUtility.DisplayProgressBar("ThunderKit", $"Downloading Cpp2IL {version}...", 0.5f);
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "ThunderKit");
                    client.DownloadFile(url, destination);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ThunderKit] Failed to download Cpp2IL from {url}: {e.Message}");
                try { if (File.Exists(destination)) File.Delete(destination); } catch { }
                return null;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            MarkExecutable(destination);
            Debug.Log($"[ThunderKit] Downloaded Cpp2IL {version} to {destination}");
            return destination;
        }

        static string CachedExecutablePath()
        {
            var suffix = PlatformSuffix();
            if (suffix == null)
                return null;
            var version = EditorPrefs.GetString(VersionPrefKey, DefaultVersion);
            return Path.Combine(CacheDirectory, $"Cpp2IL-{version}-{suffix}");
        }

        // Self-contained release asset suffix for the current Editor platform (x64).
        // ARM64 hosts should override the URL via the DownloadUrl EditorPref.
        static string PlatformSuffix()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor: return "Windows.exe";
                case RuntimePlatform.LinuxEditor: return "Linux";
                case RuntimePlatform.OSXEditor: return "OSX";
                default: return null;
            }
        }

        // The Linux/OSX assets are downloaded without the executable bit; set it.
        static void MarkExecutable(string path)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
                return;
            try
            {
                using (var chmod = Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }))
                {
                    chmod.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ThunderKit] Could not mark Cpp2IL executable ({path}): {e.Message}");
            }
        }

        [MenuItem(Constants.ThunderKitMenuRoot + "Acquire Cpp2IL", priority = Constants.ThunderKitMenuPriority)]
        static void AcquireMenu()
        {
            var path = EnsureExecutable();
            if (!string.IsNullOrEmpty(path))
                Debug.Log($"[ThunderKit] Cpp2IL ready at {path}");
        }

        [MenuItem(Constants.ThunderKitMenuRoot + "Set Cpp2IL Executable", priority = Constants.ThunderKitMenuPriority)]
        static void SetExecutable()
        {
            var current = EditorPrefs.GetString(ExecutablePrefKey, "");
            var directory = string.IsNullOrEmpty(current) ? "" : Path.GetDirectoryName(current);
            var path = EditorUtility.OpenFilePanel("Select Cpp2IL Executable", directory, "");
            if (string.IsNullOrEmpty(path))
                return;
            EditorPrefs.SetString(ExecutablePrefKey, path);
            Debug.Log($"[ThunderKit] Cpp2IL executable set to {path}");
        }
    }
}
