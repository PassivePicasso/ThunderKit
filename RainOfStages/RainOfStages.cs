using BepInEx;
using RainOfStages.Campaign;
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
using static R2API.LobbyConfigAPI;
using Path = System.IO.Path;
using R2API.Utils;
using R2API;

namespace RainOfStages.Plugin
{

    //This attribute is required, and lists metadata for your plugin.
    //The GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config). I like to use the java package notation, which is "com.[your name here].[your plugin name here]"

    //The name is the name of the plugin that's displayed on load, and the version number just specifies what version the plugin is.
    [BepInPlugin("com.PassivePicasso.RainOfStages", "RainOfStages", "2020.1.0")]
    [BepInDependency("com.bepis.r2api")]
    [R2APISubmoduleDependency(nameof(LobbyConfigAPI))]
    public class RainOfStages : BaseUnityPlugin
    {

        public static RainOfStages Instance { get; private set; }
        public static event EventHandler Initialized;

        private List<AssetBundle> LoadedScenes;
        private List<SceneDef> sceneDefList;

        public static List<CampaignDefinition> Campaigns;

        private CampaignDefinition CurrentCampaign;

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            Logger.LogInfo("Initializing MapSystem");
            LoadedScenes = new List<AssetBundle>();
            Campaigns = new List<CampaignDefinition>();
            sceneDefList = new List<SceneDef>();

            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var workingDirectory = Path.GetDirectoryName(assemblyLocation);
            Logger.LogInfo(workingDirectory);

            var dir = new DirectoryInfo(workingDirectory);
            while (dir != null && dir.Name != "plugins") dir = dir.Parent;

            if (dir == null) throw new ArgumentException(@"invalid plugin path detected, could not find expected ""plugins"" folder in parent tree");

            var campaignManifest = dir.GetFiles("campaignmanifest", SearchOption.AllDirectories);

            //var libraries = sceneDefinitionBundles.Select(sdb => sdb.DirectoryName).Distinct().SelectMany(sdPath => Directory.EnumerateFiles(sdPath, $"*.dll")).Distinct();
            //foreach (var lib in libraries)
            //    if (Directory.Exists(lib))
            //    {
            //        Assembly.LoadFrom(lib);
            //        Logger.LogInfo($"Loaded assembly: {lib}");
            //    }

            foreach (var definitionBundle in campaignManifest)
            {
                Logger.LogInfo($"Loading Scene Definitions: {definitionBundle}");
                var definitionsBundle = AssetBundle.LoadFromFile($"{definitionBundle}");
                var sceneDefinitions = definitionsBundle.LoadAllAssets<CustomSceneDefProxy>();

                foreach (var sceneDefProxy in sceneDefinitions)
                {
                    string path = Path.Combine(definitionBundle.DirectoryName, sceneDefProxy.name.ToLower());
                    var sceneAsset = AssetBundle.LoadFromFile(path);
                    LoadedScenes.Add(sceneAsset);
                }

                sceneDefList.AddRange(sceneDefinitions.Select(sdp => sdp.ToSceneDef()));
                Logger.LogInfo($"Created and Loaded {sceneDefinitions.Length} SceneDefs from Definitions File {definitionBundle}");

                var campaignDefinitions = definitionsBundle.LoadAllAssets<CampaignDefinition>();
                Campaigns.AddRange(campaignDefinitions);
                Logger.LogInfo($"Created and Loaded {campaignDefinitions.Length} CampaignDefinitions from Definitions File {definitionBundle}");
            }

            //CampaignManager.loade
            CurrentCampaign = Campaigns.FirstOrDefault();

            SetupCustomStageLoading();

            Instance = this;

            Initialized?.Invoke(this, EventArgs.Empty);

            LobbyCategory lobbyCategory = new LobbyCategory("Campaigns", Color.magenta, "Choose your campaign");
            LobbyRule<CampaignDefinition> lobbyRule = new LobbyRule<CampaignDefinition>();

            foreach(var campaign in Campaigns)
            {
                lobbyRule.AddChoice(campaign, campaign.Name, campaign.Description, Color.red, Color.black, campaign.Name);
            }

            lobbyCategory.PushRule<CampaignDefinition>(lobbyRule);
        }

        private void SetupCustomStageLoading()
        {
            Logger.LogInfo("StartUp Script Started");
            On.RoR2.Run.PickNextStageScene += Run_PickNextStageScene;
            On.RoR2.Navigation.MapNodeGroup.Bake += MapNodeGroup_Bake;

            SceneCatalog.getAdditionalEntries += DopeItUp;

            Logger.LogInfo($"Loaded {sceneDefList.Count} SceneDefs");
        }


        private void Run_PickNextStageScene(On.RoR2.Run.orig_PickNextStageScene orig, Run self, SceneDef[] choices)
        {
            if (CurrentCampaign == null) orig(self, choices);
            else
                self.nextStageScene = CurrentCampaign.PickNextScene(self.nextStageRng, self);
        }

        private void LogSceneInformation(SceneDef validStage)
        {
            Logger.LogInfo($"############ {validStage.nameToken} ############");
            Logger.LogInfo($"   Object name: {validStage.name}");
            Logger.LogInfo($"    Scene Type: {validStage.sceneType}");
            Logger.LogInfo($"   Stage Order: {validStage.stageOrder}");
            Logger.LogInfo($"    Scene Name: {validStage.baseSceneName}");
            Logger.LogInfo($"Subtitle Token: {validStage.subtitleToken}");
            Logger.LogInfo("\r\n");
        }

        private void DopeItUp(List<SceneDef> obj)
        {
            Logger.LogInfo("Loading additional scenes");
            obj.AddRange(sceneDefList);
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