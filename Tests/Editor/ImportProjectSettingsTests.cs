using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ThunderKit.Core.Config.Common;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThunderKitTests
{
    // Tier B-ish — ProjectSettings import (real export, generated fixtures).
    //
    // Drives ImportProjectSettings.ExportProjectSettings against a per-version
    // globalgamemanagers and the committed AssetRipper class data (classdata.tpk),
    // exercising the full version-resolution + YAML export path. Deterministic and offline
    // (no network, no AssetDatabase mutation), so it runs in CI across the whole Unity
    // editor matrix rather than being tagged [Category("Integration")].
    //
    // Cross-version contract, split by what is guaranteed:
    //  * On EVERY editor: resolving a class database from the tpk for the running Unity
    //    version and loading the game's globalgamemanagers must succeed (no
    //    UnsupportedClassDataException); any files produced must be valid. This proves the
    //    tpk acquisition/closest-version selection works on each matrix editor.
    //  * On a MATCHING editor (running major.minor == a fixture's): the core settings must
    //    actually export. ThunderKit assumes the importing editor matches the game's build
    //    version, so full behavioral correctness is asserted only there.
    //
    // Fixtures are auto-discovered under Fixtures/GlobalGameManagers/<unity-version>/ and
    // are produced by the generate-project-settings-fixtures workflow — one
    // globalgamemanagers built by each matrix editor, so every matrix job exercises an
    // exact class-data match. See Fixtures/GlobalGameManagers/README.md.
    [TestFixture]
    public class ImportProjectSettingsTests
    {
        const string FixturesFolderSuffix = "/Tests/Editor/Fixtures";

        // Settings present in every Unity version's globalgamemanagers as simple, stable
        // value types. On an exact (or near) class-data match they always export, so they
        // are the default "must export" set when a fixture does not pin its own
        // expectedSettings. PlayerSettings (ProjectSettings) and GraphicsSettings are
        // deliberately excluded: their layout shifts even across patch releases.
        static readonly string[] CoreSettings =
        {
            "AudioManager", "TimeManager", "TagManager", "DynamicsManager", "QualitySettings",
        };

        [Serializable]
        class FixtureManifest
        {
            public string unityVersion;
            public string description;
            public string[] expectedSettings;
        }

        // Absolute path to the Fixtures directory, located via the committed classdata.tpk
        // so it works whether the package is embedded (Assets/Packages) or in the
        // PackageCache. Null when the fixtures are absent (e.g. Git LFS not pulled).
        static string FixturesRoot()
        {
            foreach (var guid in AssetDatabase.FindAssets("classdata"))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith($"{FixturesFolderSuffix}/classdata.tpk", StringComparison.OrdinalIgnoreCase))
                    return Path.GetDirectoryName(Path.GetFullPath(assetPath));
            }
            return null;
        }

        // One case per fixture folder that has both a globalgamemanagers and a fixture.json.
        // Yields a single Ignored case when no fixtures are present so the suite stays green
        // rather than erroring on an empty TestCaseSource.
        static IEnumerable<TestCaseData> Fixtures()
        {
            var root = FixturesRoot();
            var ggmRoot = root == null ? null : Path.Combine(root, "GlobalGameManagers");

            var any = false;
            if (ggmRoot != null && Directory.Exists(ggmRoot))
            {
                foreach (var dir in Directory.GetDirectories(ggmRoot))
                {
                    var ggm = Path.Combine(dir, "globalgamemanagers");
                    var manifest = Path.Combine(dir, "fixture.json");
                    if (File.Exists(ggm) && File.Exists(manifest))
                    {
                        any = true;
                        yield return new TestCaseData(dir).SetName($"Export_{Path.GetFileName(dir)}");
                    }
                }
            }

            if (!any)
                yield return new TestCaseData((string)null)
                    .Ignore("No ProjectSettings fixtures found under Tests/Editor/Fixtures/GlobalGameManagers (Git LFS not pulled?).");
        }

        [Test, TestCaseSource(nameof(Fixtures))]
        public void ExportProjectSettings_ResolvesAcrossVersions(string fixtureDir)
        {
            var root = FixturesRoot();
            Assert.That(root, Is.Not.Null, "Fixtures root could not be located.");

            var tpkPath = Path.Combine(root, "classdata.tpk");
            Assert.That(File.Exists(tpkPath), Is.True, $"Missing class data fixture: {tpkPath}");

            var globalGameManagersPath = Path.Combine(fixtureDir, "globalgamemanagers");

            if (IsLfsPointer(tpkPath) || IsLfsPointer(globalGameManagersPath))
                Assert.Ignore("Binary fixtures are unresolved Git LFS pointers; run `git lfs pull` to fetch them.");
            var manifest = JsonUtility.FromJson<FixtureManifest>(File.ReadAllText(Path.Combine(fixtureDir, "fixture.json")));

            var outputDirectory = Path.Combine(
                Directory.GetCurrentDirectory(), "Temp",
                $"ImportProjectSettingsTests_{Path.GetFileName(fixtureDir)}");
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, true);

            var importer = ScriptableObject.CreateInstance<ImportProjectSettings>();
            List<string> written;
            try
            {
                // Mirrors production: the export is driven by the *editor's* Unity version,
                // which selects the closest class database the tpk provides.
                written = importer.ExportProjectSettings(
                    tpkPath, globalGameManagersPath, outputDirectory, Application.unityVersion);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(importer);
            }

            try
            {
                // Guaranteed on every editor: resolution + class DB load succeeded and the
                // per-setting export never throws (failures are skipped, not raised).
                Assert.That(written, Is.Not.Null, "Export returned null instead of a (possibly empty) file list.");

                foreach (var file in written)
                {
                    Assert.That(File.Exists(file), Is.True, $"Export reported '{file}' but it does not exist.");
                    Assert.That(new FileInfo(file).Length, Is.GreaterThan(0), $"Exported setting '{file}' is empty.");
                    var firstLine = File.ReadLines(file).FirstOrDefault() ?? string.Empty;
                    Assert.That(firstLine, Does.StartWith("%YAML").Or.StartWith("---"),
                        $"Exported setting '{file}' is not YAML (first line: '{firstLine}').");
                }

                var exportedNames = new HashSet<string>(
                    written.Select(Path.GetFileNameWithoutExtension), StringComparer.OrdinalIgnoreCase);

                if (RunningEditorMatches(manifest.unityVersion))
                {
                    // Same major.minor as the captured game: the export must be correct.
                    Assert.That(written.Count, Is.GreaterThan(0),
                        $"No settings exported on matching editor {Application.unityVersion}.");

                    var expectedSettings = manifest.expectedSettings != null && manifest.expectedSettings.Length > 0
                        ? manifest.expectedSettings
                        : CoreSettings;
                    foreach (var expected in expectedSettings)
                        Assert.That(exportedNames, Contains.Item(expected),
                            $"Expected setting '{expected}' was not exported under matching editor {Application.unityVersion}. " +
                            $"Exported: {string.Join(", ", exportedNames)}.");
                }
                else
                {
                    // Different version: type layouts may diverge, so some (or all) settings
                    // can be skipped. We only require that resolution succeeded and whatever
                    // was produced is valid (asserted above). Surface the count for triage.
                    Assert.Pass($"Editor {Application.unityVersion} differs from fixture {manifest.unityVersion}; " +
                        $"resolution succeeded, {written.Count} setting(s) exported.");
                }
            }
            finally
            {
                if (Directory.Exists(outputDirectory))
                    Directory.Delete(outputDirectory, true);
            }
        }

        // A Git LFS pointer is a small text stub beginning with the LFS version line.
        // Real fixtures are multi-KB binaries, so a tiny file starting with that line
        // means LFS content was never fetched.
        static bool IsLfsPointer(string path)
        {
            if (!File.Exists(path) || new FileInfo(path).Length > 1024)
                return false;
            return (File.ReadLines(path).FirstOrDefault() ?? string.Empty)
                .StartsWith("version https://git-lfs", StringComparison.Ordinal);
        }

        static bool RunningEditorMatches(string fixtureUnityVersion)
        {
            return ClassDataManager.TryParseUnityVersion(Application.unityVersion, out var em, out var en, out _)
                && ClassDataManager.TryParseUnityVersion(fixtureUnityVersion, out var fm, out var fn, out _)
                && em == fm && en == fn;
        }
    }
}
