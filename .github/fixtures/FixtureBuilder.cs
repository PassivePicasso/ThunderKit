// CI-only tooling — copied into a throwaway Unity project by
// .github/workflows/generate-project-settings-fixtures.yml. NOT part of the
// shipped package (it lives under .github/, outside any package assembly).
//
// Builds a minimal empty StandaloneLinux64 player and copies the
// globalgamemanagers that the build produces into <project>/Fixture/, alongside a
// fixture.json recording the exact editor version. The workflow then uploads that
// folder as the per-version ProjectSettings import fixture.
//
// A globalgamemanagers is only ever emitted by an actual player build (no editor
// API produces one at rest) and is pure engine/project settings, so an empty
// project yields a clean, representative file with no game content and no
// dependency on ThunderKit being present.

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FixtureBuilder
{
    // Entry point invoked via -buildMethod / -executeMethod from GameCI.
    public static void Build()
    {
        var exitCode = 1;
        try
        {
            exitCode = Run() ? 0 : 1;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FixtureBuilder] Fixture generation threw: {e}");
            exitCode = 1;
        }
        EditorApplication.Exit(exitCode);
    }

    static bool Run()
    {
        var version = Application.unityVersion;

        // A build needs at least one scene; generate an empty one rather than
        // requiring any authored asset.
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        const string scenePath = "Assets/_FixtureScene.unity";
        if (!EditorSceneManager.SaveScene(scene, scenePath))
        {
            Debug.LogError("[FixtureBuilder] Failed to save the temporary build scene.");
            return false;
        }

        var buildDir = Path.Combine(Path.GetTempPath(), "fixturebuild");
        if (Directory.Exists(buildDir))
            Directory.Delete(buildDir, true);
        Directory.CreateDirectory(buildDir);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { scenePath },
            // The _Data folder (and thus globalgamemanagers) is named after this file.
            locationPathName = Path.Combine(buildDir, "fixture.x86_64"),
            target = BuildTarget.StandaloneLinux64,
            targetGroup = BuildTargetGroup.Standalone,
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.LogError($"[FixtureBuilder] Player build did not succeed: {report.summary.result}.");
            return false;
        }

        var ggm = Directory
            .GetFiles(buildDir, "globalgamemanagers", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (ggm == null)
        {
            Debug.LogError($"[FixtureBuilder] Build succeeded but no globalgamemanagers was found under {buildDir}.");
            return false;
        }

        // Application.dataPath is <project>/Assets; the fixture goes next to the project
        // root so the workflow can pick it up regardless of the batchmode working dir.
        var outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Fixture"));
        if (Directory.Exists(outputDir))
            Directory.Delete(outputDir, true);
        Directory.CreateDirectory(outputDir);

        File.Copy(ggm, Path.Combine(outputDir, "globalgamemanagers"), true);
        File.WriteAllText(
            Path.Combine(outputDir, "fixture.json"),
            "{\n  \"unityVersion\": \"" + version + "\"\n}\n");

        Debug.Log($"[FixtureBuilder] Wrote globalgamemanagers fixture for Unity {version} to {outputDir}.");
        return true;
    }
}
