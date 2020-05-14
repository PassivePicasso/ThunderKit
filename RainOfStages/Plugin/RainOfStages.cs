using BepInEx;
using MonoMod.RuntimeDetour.HookGen;
using RainOfStages.Proxy;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Path = System.IO.Path;

namespace RainOfStages.Plugin
{
    //This attribute is required, and lists metadata for your plugin.
    //The GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config). I like to use the java package notation, which is "com.[your name here].[your plugin name here]"

    //The name is the name of the plugin that's displayed on load, and the version number just specifies what version the plugin is.
    [BepInPlugin("com.PassivePicasso.RainOfStages", "RainOfStages", "2020.1.0")]
    public class RainOfStages : BaseUnityPlugin
    {
        private const int GameBuild = 4892828;

        class ManifestMap
        {
            public FileInfo File;
            public string[] Content;
        }
        private const string NamePrefix = "      Name: ";
        private static FieldInfo[] sceneDefFields = typeof(SceneDef).GetFields(BindingFlags.Public | BindingFlags.Instance);

        public static RainOfStages Instance { get; private set; }
        public static event EventHandler Initialized;

        private List<AssetBundle> LoadedScenes;
        private List<SceneDef> sceneDefList;

        public RainOfStages()
        {
            Logger.LogWarning("Constructor Executed");
            RoR2Application.isModded = true;
            try
            {
                var consoleRedirectorType = typeof(RoR2.RoR2Application).GetNestedType("UnitySystemConsoleRedirector", BindingFlags.NonPublic);
                Logger.LogMessage($"{consoleRedirectorType.FullName} found in {typeof(RoR2Application).FullName}");
                var redirect = consoleRedirectorType.GetMethod("Redirect", BindingFlags.Public | BindingFlags.Static);
                Logger.LogMessage($"{redirect.Name}() found in {consoleRedirectorType.FullName}");
                HookEndpointManager.Add<Hook>(redirect, (Hook)(_ => { }));
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to redirect console");
            }


            var qpbcStart = typeof(QuickPlayButtonController).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            var digmOnEnable = typeof(DisableIfGameModded).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.Public);
            var sdAwake = typeof(SceneDef).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            var scInit = typeof(SceneCatalog).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic);

            HookEndpointManager.Add<Hook<QuickPlayButtonController>>(qpbcStart, (Hook<QuickPlayButtonController>)DisableQuickPlay);
            HookEndpointManager.Add<Hook<DisableIfGameModded>>(digmOnEnable, (Hook<DisableIfGameModded>)DisableIfGameModded_Start);
            HookEndpointManager.Add<Hook<SceneDef>>(sdAwake, (Hook<SceneDef>)SceneDef_Awake);
            HookEndpointManager.Add<Hook>(scInit, (Hook)Init);

            void Init(Action orig)
            {
                orig();

                HookEndpointManager.Remove<Hook>(scInit, (Hook)Init);

                var lookups = SceneCatalog.allSceneDefs.ToDictionary(sd => sd.baseSceneName);

                Logger.LogInfo("Lodded dictionary for sceneNameOverride doping");

                IEnumerable<SceneDefinition> sceneDefinitions = sceneDefList.OfType<SceneDefinition>();

                {
                    var maps = sceneDefinitions.SelectMany(destination => destination.destionationInjections.Select(origin => (destination, origin)));
                    var mapGroups = maps.GroupBy(map => map.origin.baseSceneName);

                    foreach (var mapGroup in mapGroups)
                    {
                        var destinations = lookups[mapGroup.Key].destinations = mapGroup.Select(map => map.destination as SceneDef).ToArray();
                        foreach (var destination in destinations)
                            Logger.LogMessage($"Added destination {destination.baseSceneName} to SceneDef {mapGroup.Key}");
                    }
                }

                {
                    var maps = sceneDefinitions.SelectMany(overridingScene => overridingScene.reverseSceneNameOverrides.Select(overridedScene => (overridingScene, overridedScene)));
                    var mapGroups = maps.GroupBy(map => map.overridedScene.baseSceneName);

                    foreach (var mapGroup in mapGroups)
                    {
                        var overridingScenes = lookups[mapGroup.Key].sceneNameOverrides = mapGroup.Select(map => map.overridingScene.baseSceneName).ToList();

                        foreach (var overridingScene in overridingScenes)
                            Logger.LogMessage($"Added override {overridingScene} to SceneDef {mapGroup.Key}");
                    }
                }
            }
        }

        public void Awake()
        {
            Logger.LogInfo("Initializing Rain of Stages");
            LoadedScenes = new List<AssetBundle>();
            sceneDefList = new List<SceneDef>();

            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var workingDirectory = Path.GetDirectoryName(assemblyLocation);
            Logger.LogInfo(workingDirectory);

            var dir = new DirectoryInfo(workingDirectory);
            while (dir != null && dir.Name != "plugins") dir = dir.Parent;

            if (dir == null) throw new ArgumentException(@"invalid plugin path detected, could not find expected ""plugins"" folder in parent tree");
            Func<string, bool> hasNameEntry = line => line.StartsWith(NamePrefix);

            var manifestMaps = dir.GetFiles("*.manifest", SearchOption.AllDirectories)
                               .Select(manifestFile => new ManifestMap { File = manifestFile, Content = File.ReadAllLines(manifestFile.FullName) })
                               .Where(mfm => mfm.Content.Any(line => line.StartsWith("AssetBundleManifest:")))
                               .Where(mfm => mfm.Content.Any(line => line.Contains("stagemanifest")))
                               .ToArray();

            Logger.LogInfo($"Loaded Rain of Stages compatible AssetBundles");
            foreach (var mfm in manifestMaps)
            {
                try
                {
                    var directory = mfm.File.DirectoryName;
                    var filename = Path.GetFileNameWithoutExtension(mfm.File.FullName);
                    var abmPath = Path.Combine(directory, filename);
                    var namedBundle = AssetBundle.LoadFromFile(abmPath);
                    var manifest = namedBundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
                    var dependentBundles = manifest.GetAllAssetBundles();
                    foreach (var definitionBundle in dependentBundles)
                    {
                        try
                        {
                            var bundlePath = Path.Combine(directory, definitionBundle);
                            var bundle = AssetBundle.LoadFromFile(bundlePath);

                            if (bundle.isStreamedSceneAssetBundle)
                            {
                                LoadedScenes.Add(bundle);
                                Logger.LogInfo($"Loaded Scene {definitionBundle}");
                            }
                            else
                            {
                                var sceneDefinitions = bundle.LoadAllAssets<SceneDefinition>();
                                if (sceneDefinitions.Length > 0)
                                {
                                    sceneDefList.AddRange(sceneDefinitions);
                                    Logger.LogInfo($"Loaded Scene Definitions {sceneDefinitions.Select(sd => sd.name).Aggregate((a, b) => $"{a}, {b}")}");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }
            }

            SceneCatalog.getAdditionalEntries += ProvideAdditionalSceneDefs;

            Instance = this;


            Initialized?.Invoke(this, EventArgs.Empty);
        }

        private static void DisableQuickPlay(Action<QuickPlayButtonController> orig, QuickPlayButtonController self) => self.gameObject.SetActive(false);
        private static void DisableIfGameModded_Start(Action<DisableIfGameModded> orig, DisableIfGameModded self) => self.gameObject.SetActive(false);

        private void ProvideAdditionalSceneDefs(List<SceneDef> sceneDefinitions)
        {
            Logger.LogMessage("Loading additional scenes w0000000000000000000000000000000000000000000000000000000t");
            sceneDefinitions.AddRange(sceneDefList);
        }

        private void SceneDef_Awake(Action<SceneDef> orig, SceneDef self)
        {
            if (self is SceneDefReference sdr)
            {
                var def = Resources.Load<SceneDef>($"SceneDefs/{sdr.name}");
                foreach (var field in sceneDefFields)
                    field.SetValue(self, field.GetValue(def));
            }
            orig(self);
        }

        private void PrintHieriarchy(Transform transform, int indent = 0)
        {
            string indentString = indent > 0 ? Enumerable.Repeat(" ", indent).Aggregate((a, b) => $"{a}{b}") : "";

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform childTransform = transform.GetChild(i);
                var message = $"{indentString}{childTransform?.gameObject?.name}";
                Logger.LogMessage(message);
                PrintHieriarchy(childTransform, indent + 1);
            }
        }
    }
}