using BepInEx;
using RainOfStages.Proxies;
using RoR2;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;
using Path = System.IO.Path;

namespace RainOfStages.Plugin
{

    //This attribute is required, and lists metadata for your plugin.
    //The GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config). I like to use the java package notation, which is "com.[your name here].[your plugin name here]"
    //The name is the name of the plugin that's displayed on load, and the version number just specifies what version the plugin is.
    [BepInPlugin("com.PassivePicasso.RainOfStages", "RainOfStages", "2019.1.1")]

    public class RainOfStages : BaseUnityPlugin
    {

        private List<AssetBundle> LoadedScenes;
        private List<SceneDef> sceneDefList;
        private SceneDef[] validStages;


        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            Logger.LogInfo("Initializing MapSystem");
            LoadedScenes = new List<AssetBundle>();

            sceneDefList = new List<SceneDef>();
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var workingDirectory = Path.GetDirectoryName(assemblyLocation);
            Logger.LogInfo(workingDirectory);

            var dir = new DirectoryInfo(workingDirectory);
            while (dir != null && dir.Name != "plugins") dir = dir.Parent;

            if (dir == null) throw new ArgumentException(@"invalid plugin path detected, could not find expected ""plugins"" folder in parent tree");

            var sceneDefinitionBundles = dir.GetFiles("mapsystem_scenedefinitions", SearchOption.AllDirectories);
            foreach (var sceneDefinitionBundle in sceneDefinitionBundles)
            {
                Logger.LogInfo($"Loading Scene Definitions: {sceneDefinitionBundle}");
                var definitionsBundle = AssetBundle.LoadFromFile($"{sceneDefinitionBundle}");

                var sceneDefinitions = definitionsBundle.LoadAllAssets<CustomSceneDefProxy>();

                foreach (var sceneDefProxy in sceneDefinitions)
                {
                    var name = sceneDefProxy.name;
                    var sceneAsset = AssetBundle.LoadFromFile(Path.Combine(sceneDefinitionBundle.DirectoryName, name.ToLower()));
                    LoadedScenes.Add(sceneAsset);

                    var assembly = Directory.EnumerateFiles(sceneDefinitionBundle.DirectoryName, $"{name}.dll".ToLower()).FirstOrDefault();
                    if (string.IsNullOrEmpty(assembly)) continue;

                    Assembly.LoadFrom(assembly);
                }

                Logger.LogInfo($"Loaded {sceneDefinitions.Length} SceneDefProxies");
                //foreach(var definition in sceneDefinitions)
                //    Logger.LogInfo($"Loaded {definition.sceneType} {definition.nameToken}");

                sceneDefList.AddRange(sceneDefinitions.Select(sdp => sdp.ToSceneDef()));
                Logger.LogInfo($"Created and Loaded {sceneDefinitions.Length} SceneDefs from Scene Definitions File {sceneDefinitionBundle}");
            }

            SetupCustomStageLoading();
        }

        private void SetupCustomStageLoading()
        {
            Logger.LogInfo("StartUp Script Started");

            On.RoR2.Run.PickNextStageScene += Run_PickNextStageScene;
            On.RoR2.Stage.OnEnable += Stage_OnEnable;
            On.RoR2.Navigation.MapNodeGroup.Bake += MapNodeGroup_Bake;

            SceneCatalog.getAdditionalEntries += DopeItUp;

            Logger.LogInfo($"Loaded {sceneDefList.Count} SceneDefs");
        }

        private void Run_PickNextStageScene(On.RoR2.Run.orig_PickNextStageScene orig, Run self, SceneDef[] choices)
        {
            if (validStages == null)
            {
                //Logger.LogInfo($"############ All SceneDefs ############");
                //foreach (var validStage in SceneCatalog.allSceneDefs) LogSceneInformation(validStage);


                //validStages = SceneCatalog.allSceneDefs.Where<SceneDef>((Func<SceneDef, bool>)(sceneDef => sceneDef.sceneType == SceneType.Stage)).ToArray<SceneDef>();
                //Logger.LogInfo($"############ Valid Stages ############");
                //foreach (var validStage in validStages) LogSceneInformation(validStage);
                validStages = sceneDefList.ToArray();
            }
            //if (self.ruleBook.stageOrder == StageOrder.Normal)
            //    self.nextStageScene = self.nextStageRng.NextElementUniform<SceneDef>(choices);
            //else
            self.nextStageScene = self.nextStageRng.NextElementUniform(((IEnumerable<SceneDef>)validStages).Where(sceneDef => sceneDef != self.nextStageScene).ToArray());
        }

        private void LogSceneInformation(SceneDef validStage)
        {
            Logger.LogInfo($"############ {validStage.nameToken} ############");
            Logger.LogInfo($"   Object name: {validStage.name}");
            Logger.LogInfo($"    Scene Type: {validStage.sceneType}");
            Logger.LogInfo($"   Stage Order: {validStage.stageOrder}");
            Logger.LogInfo($"    Scene Name: {validStage.sceneName}");
            Logger.LogInfo($"Subtitle Token: {validStage.subtitleToken}");
            Logger.LogInfo("\r\n");
        }

        private void DopeItUp(List<SceneDef> obj)
        {
            Logger.LogInfo("Loading additional scenes");
            obj.AddRange(sceneDefList);
        }

        private void Stage_OnEnable(On.RoR2.Stage.orig_OnEnable orig, Stage self)
        {
            orig(self);

            var initializer = GameObject.Find("StageInitializer");
            if (!initializer)
            {
                Logger.LogError("Failed to get StageInitializer add StageInitializer to scene");
                return;
            }
            var rootEnabler = initializer.GetComponent<RootEnabler>();
            rootEnabler.StartScene();
        }


        private void MapNodeGroup_Bake(On.RoR2.Navigation.MapNodeGroup.orig_Bake orig, MapNodeGroup self, NodeGraph nodeGraph)
        {
            List<MapNode> nodes = self.GetNodes();

            ReadOnlyCollection<MapNode> readonlyNodes = nodes.AsReadOnly();

            foreach (var node in nodes) node.BuildLinks(readonlyNodes, self.graphType);

            nodeGraph.SetNodes(readonlyNodes, CalculateLineOfSight(nodes));
        }

        private ReadOnlyCollection<SerializableBitArray> CalculateLineOfSight(List<MapNode> nodes)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int testSetSize = nodes.Count * nodes.Count;

            NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(testSetSize, Allocator.Temp);
            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(testSetSize, Allocator.Temp);

            for (int index1 = 0; index1 < nodes.Count; ++index1)
            {
                var offset = index1 * nodes.Count;
                MapNode mapNode = nodes[index1];
                for (int index2 = 0; index2 < nodes.Count; ++index2)
                {
                    MapNode other = nodes[index2];

                    var origin = mapNode.transform.position + Vector3.up;
                    var destination = other.transform.position + Vector3.up;
                    var direction = destination - origin;
                    commands[offset + index2] = new RaycastCommand(origin, direction, Vector3.Distance(origin, destination), LayerIndex.world.mask, 1);
                }
            }

            stopwatch.Stop();
            Logger.LogInfo($"Prepared {testSetSize} RaycastCommands in {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Reset();
            stopwatch.Start();

            var handle = RaycastCommand.ScheduleBatch(commands, results, 1);
            handle.Complete();

            stopwatch.Stop();
            Logger.LogInfo($"Executed {testSetSize} RaycastCommands in {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Reset();
            stopwatch.Start();

            var serializableBitArrayList = new List<SerializableBitArray>(nodes.Count);
            for (int index1 = 0; index1 < nodes.Count; ++index1)
            {
                var offset = index1 * nodes.Count;
                SerializableBitArray serializableBitArray = new SerializableBitArray(nodes.Count);
                for (int index2 = 0; index2 < nodes.Count; ++index2)
                    serializableBitArray[index2] = results[offset + index2].collider == null;

                serializableBitArrayList.Add(serializableBitArray);
            }
            stopwatch.Stop();
            Logger.LogInfo($"Processed {testSetSize} RaycastHits into BitArray in {stopwatch.ElapsedMilliseconds}ms");

            results.Dispose();
            commands.Dispose();

            return serializableBitArrayList.AsReadOnly();
        }

    }
}