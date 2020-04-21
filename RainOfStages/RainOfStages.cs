using BepInEx;
using RainOfStages.Campaign;
using RainOfStages.Proxy;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using NodeGraph = RainOfStages.Proxy.NodeGraph;
using Path = System.IO.Path;

namespace RainOfStages.Plugin
{

    //This attribute is required, and lists metadata for your plugin.
    //The GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config). I like to use the java package notation, which is "com.[your name here].[your plugin name here]"

    //The name is the name of the plugin that's displayed on load, and the version number just specifies what version the plugin is.
    [BepInPlugin("com.PassivePicasso.RainOfStages", "RainOfStages", "2020.1.0")]
    [BepInDependency("com.bepis.r2api")]
    public class RainOfStages : BaseUnityPlugin
    {
        private static FieldInfo[] sceneDefFields = typeof(SceneDef).GetFields(BindingFlags.Public | BindingFlags.Instance);

        public static RainOfStages Instance { get; private set; }
        public static event EventHandler Initialized;

        private List<AssetBundle> LoadedScenes;
        private List<SceneDef> sceneDefList;
        private List<NodeGraph> nodeGraphs;

        public static List<CampaignDefinition> Campaigns;

        private CampaignDefinition CurrentCampaign;
        private int selectedCampaignIndex = 0;

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            Logger.LogInfo("Initializing MapSystem");
            LoadedScenes = new List<AssetBundle>();
            Campaigns = new List<CampaignDefinition>();
            sceneDefList = new List<SceneDef>();
            nodeGraphs = new List<NodeGraph>();

            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var workingDirectory = Path.GetDirectoryName(assemblyLocation);
            Logger.LogInfo(workingDirectory);

            var dir = new DirectoryInfo(workingDirectory);
            while (dir != null && dir.Name != "plugins") dir = dir.Parent;

            if (dir == null) throw new ArgumentException(@"invalid plugin path detected, could not find expected ""plugins"" folder in parent tree");


            var campaignManifest = dir.GetFiles("campaignmanifest", SearchOption.AllDirectories);

            On.RoR2.SceneDef.Awake += SceneDef_Awake;

            foreach (var definitionBundle in campaignManifest)
            {
                Logger.LogInfo($"Loading Scene Definitions: {definitionBundle}");
                var definitionsBundle = AssetBundle.LoadFromFile($"{definitionBundle}");
                var sceneDefinitions = definitionsBundle.LoadAllAssets<CustomSceneDefProxy>();
                var bundleGraphs = definitionsBundle.LoadAllAssets<NodeGraph>();
                nodeGraphs.AddRange(bundleGraphs);

                foreach (var sceneDef in sceneDefinitions)
                {
                    string path = Path.Combine(definitionBundle.DirectoryName, sceneDef.name.ToLower());
                    var sceneAsset = AssetBundle.LoadFromFile(path);
                    LoadedScenes.Add(sceneAsset);
                }

                sceneDefList.AddRange(sceneDefinitions.Select(sdp => sdp));
                Logger.LogInfo($"Created and Loaded {sceneDefinitions.Length} SceneDefs from Definitions File {definitionBundle}");

                var campaignDefinitions = definitionsBundle.LoadAllAssets<CampaignDefinition>();
                Campaigns.AddRange(campaignDefinitions);
                Logger.LogInfo($"Created and Loaded {campaignDefinitions.Length} CampaignDefinitions from Definitions File {definitionBundle}");
            }

            CurrentCampaign = Campaigns.FirstOrDefault(campaign => campaign.name == "RiskOfRain2Campaign") ?? Campaigns.First();
            selectedCampaignIndex = Campaigns.IndexOf(CurrentCampaign);

            SetupCustomStageLoading();

            Instance = this;

            Initialized?.Invoke(this, EventArgs.Empty);
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        private void SceneDef_Awake(On.RoR2.SceneDef.orig_Awake orig, SceneDef self)
        {
            if(self is SceneDefReference sdr)
            {
                var def = Resources.Load<SceneDef>($"SceneDefs/{sdr.name}");
                foreach (var field in sceneDefFields)
                    field.SetValue(self, field.GetValue(def));
            }
            orig(self);
        }

        private void ResolveSpawncard(DirectorCard card)
        {
            bool spawnCardResolved = false;
            RoR2.SpawnCard result = null;

            var isRoSCard = card.spawnCard.GetType().Namespace.StartsWith(nameof(RainOfStages));
            if (!isRoSCard) return;

            string cardName = card?.spawnCard?.name;
            Logger.LogMessage($"Evaluating Spawncard {cardName}");

            switch (card.spawnCard)
            {
                case Proxy.InteractableSpawnCard isc:
                    Logger.LogMessage($"Resolving Spawncard for: {cardName}");
                    result = isc.ResolveProxy();
                    spawnCardResolved = true;
                    break;

                case Proxy.CharacterSpawnCard csc:
                    Logger.LogMessage($"Resolving Spawncard for: {cardName}");
                    result = csc.ResolveProxy();
                    spawnCardResolved = true;
                    break;

                case Proxy.BodySpawnCard bsc:
                    Logger.LogMessage($"Resolving Spawncard for: {cardName}");
                    result = bsc.ResolveProxy();
                    spawnCardResolved = true;
                    break;

                case Proxy.SpawnCard sc: break;
                default:
                    break;
            }
            if (spawnCardResolved && result == null)
                Logger.LogMessage($"No Spawncard found for: {cardName}");
            else if (spawnCardResolved)
                card.spawnCard = result;
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            try
            {
                //self.gameObject.AddComponent<CampaignManager>();
                Logger.LogMessage("Adding Campaign Selector to Main Menu");
                Logger.LogMessage($"HB: {128}");

                var profileButtonGo = GameObject.Find("GenericMenuButton (Singleplayer)");
                var profileButtonText = profileButtonGo.GetComponentInChildren<HGTextMeshProUGUI>();
                var profileButtonImage = profileButtonGo.GetComponent<Image>();
                var profileButton = profileButtonGo.GetComponent<Button>();

                var profileRectTrans = profileButtonGo.GetComponent<RectTransform>();
                var buttonPanelRectTrans = profileRectTrans.parent;

                var preview = new GameObject("CampaignImage", typeof(CanvasRenderer));
                var panel = new GameObject("CampaignPanel", typeof(CanvasRenderer));
                var next = new GameObject("NextCampaign", typeof(CanvasRenderer));
                var prev = new GameObject("PrevCampaign", typeof(CanvasRenderer));
                var header = new GameObject("CampaignHeader", typeof(CanvasRenderer));

                var previewRectTrans = preview.AddComponent<RectTransform>();
                var panelRectTrans = panel.AddComponent<RectTransform>();
                var nextRectTrans = next.AddComponent<RectTransform>();
                var prevRectTrans = prev.AddComponent<RectTransform>();
                var headerRectTrans = header.AddComponent<RectTransform>();

                var previewImage = preview.AddComponent<Image>();
                var nextButtonImage = next.AddComponent<Image>();
                var prevButtonImage = prev.AddComponent<Image>();
                var panelImage = panel.AddComponent<Image>();

                CopyImageSettings(profileButtonImage, nextButtonImage);
                CopyImageSettings(profileButtonImage, prevButtonImage);
                CopyImageSettings(profileButtonImage, panelImage);

                var nextButton = next.AddComponent<Button>();
                var prevButton = prev.AddComponent<Button>();

                Color buttonNormalColor = profileButton.colors.normalColor;
                panelImage.color = new Color(buttonNormalColor.r + .1f, buttonNormalColor.g, buttonNormalColor.b, 0.75f);

                var headerText = header.AddComponent<HGTextMeshProUGUI>();
                headerText.font = HGTextMeshProUGUI.defaultLanguageFont;
                headerText.color = profileButtonText.color;
                headerText.alignment = TMPro.TextAlignmentOptions.Center;
                headerText.autoSizeTextContainer = profileButtonText.autoSizeTextContainer;
                headerText.text = "Campaign Select";

                prevButton.colors = profileButton.colors;
                nextButton.colors = profileButton.colors;

                prevButton.image = prevButtonImage;
                nextButton.image = nextButtonImage;

                nextButton.onClick.AddListener(() =>
                {
                    selectedCampaignIndex++;

                    if (selectedCampaignIndex >= Campaigns.Count) selectedCampaignIndex = 0;
                    CurrentCampaign = Campaigns[selectedCampaignIndex];

                    UpdateCampaignPreview(previewImage);
                });
                prevButton.onClick.AddListener(() =>
                {
                    selectedCampaignIndex--;
                    if (selectedCampaignIndex < 0) selectedCampaignIndex = Campaigns.Count - 1;
                    CurrentCampaign = Campaigns[selectedCampaignIndex];

                    UpdateCampaignPreview(previewImage);
                });

                headerRectTrans.SetParent(panelRectTrans);
                nextRectTrans.SetParent(panelRectTrans);
                prevRectTrans.SetParent(panelRectTrans);
                previewRectTrans.SetParent(panelRectTrans);
                panelRectTrans.SetParent(buttonPanelRectTrans);
                panelRectTrans.SetAsFirstSibling();

                ConfigureTransform(panelRectTrans, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0), new Vector3(0, 5, 0), new Vector2(profileRectTrans.sizeDelta.x, 190));

                ConfigureTransform(headerRectTrans, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 0), new Vector3(0, -40, 0), new Vector2(profileRectTrans.sizeDelta.x, 40));

                ConfigureTransform(prevRectTrans, Vector2.zero, new Vector2(0, 1), new Vector2(0, 1), new Vector3(0, -40, 0), new Vector2(30, -42));

                ConfigureTransform(nextRectTrans, new Vector2(1, 0), Vector2.one, Vector2.one, new Vector3(0, -40, 0), new Vector2(30, -42));

                ConfigureTransform(previewRectTrans, Vector2.zero, Vector2.one, new Vector2(0, 1), new Vector3(30, -43, 0), new Vector2(-60, -48));

                if (CurrentCampaign != null)
                {
                    UpdateCampaignPreview(previewImage);
                }
                else
                    Logger.LogError("Error Adding Campaign Selector to Main Menu: No Campaign Selected");


                Logger.LogMessage("Finished Adding Campaign Selector to Main Menu");
            }
            catch (Exception e)
            {
                Logger.LogError("Error Adding Campaign Selector to Main Menu");
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
            }
            finally
            {
                Logger.LogMessage("Finished Main Menu Modifications");
                orig(self);
            }
        }

        private static void ConfigureTransform(RectTransform transform, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector3 anchoredPosition3D, Vector2 sizeDelta)
        {
            transform.anchorMin = anchorMin;
            transform.anchorMax = anchorMax;
            transform.pivot = pivot;
            transform.anchoredPosition3D = anchoredPosition3D;
            transform.sizeDelta = sizeDelta;
        }

        private void UpdateCampaignPreview(Image previewImage)
        {
            previewImage.sprite = Sprite.Create(
                                    CurrentCampaign.previewTexture,
                                    new Rect(0, 0, CurrentCampaign.previewTexture.width, CurrentCampaign.previewTexture.height),
                                    Vector2.zero
                                );
        }

        void CopyImageSettings(Image from, Image to)
        {
            to.sprite = from.sprite;
            to.type = from.type;
            to.fillCenter = from.fillCenter;
            to.material = from.material;
            to.useSpriteMesh = from.useSpriteMesh;
            to.preserveAspect = from.preserveAspect;
            to.fillAmount = from.fillAmount;
            to.fillOrigin = from.fillOrigin;
            to.fillClockwise = from.fillClockwise;
            to.alphaHitTestMinimumThreshold = from.alphaHitTestMinimumThreshold;
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

        private void SetupCustomStageLoading()
        {
            Logger.LogInfo("StartUp Script Started");
            On.RoR2.Run.PickNextStageScene += Run_PickNextStageScene;

            //On.RoR2.Navigation.MapNodeGroup.Bake += MapNodeGroup_Bake;

            SceneCatalog.getAdditionalEntries += DopeItUp;

            Logger.LogInfo($"Loaded {sceneDefList.Count} SceneDefs");
        }

        private void Run_PickNextStageScene(On.RoR2.Run.orig_PickNextStageScene orig, Run self, SceneDef[] choices)
        {
            if (CurrentCampaign?.StartSegment != null)
                self.nextStageScene = CurrentCampaign.PickNextScene(self.nextStageRng, self);
            else
                orig(self, choices);
        }

        private void DopeItUp(List<SceneDef> obj)
        {
            Logger.LogInfo("Loading additional scenes");
            obj.AddRange(sceneDefList);
        }

        #region deprecated
        /*
         
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

         
         */
        #endregion
    }
}