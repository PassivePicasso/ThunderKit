using BepInEx;
using MonoMod.RuntimeDetour.HookGen;
using RainOfStages.Campaign;
using RainOfStages.Proxy;
using RoR2;
using RoR2.UI;
using RoR2.UI.MainMenu;
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
        class ManifestMap
        {
            public FileInfo File;
            public string[] Content;
        }
        private const string campaignManifestName = "campaignmanifest";
        private const string NamePrefix = "      Name: ";
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
            Func<string, bool> hasNameEntry = line => line.StartsWith(NamePrefix);

            var manifestMaps = dir.GetFiles("*.manifest", SearchOption.AllDirectories)
                               .Select(manifestFile => new ManifestMap { File = manifestFile, Content = File.ReadAllLines(manifestFile.FullName) })
                               .Where(mfm => mfm.Content.Any(line => line.StartsWith("AssetBundleManifest:")))
                               .Where(mfm => mfm.Content.Any(line => line.Contains("campaignmanifest")))
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
                                var sceneDefinitions = bundle.LoadAllAssets<CustomSceneDefProxy>();
                                if (sceneDefinitions.Length > 0)
                                {
                                    sceneDefList.AddRange(sceneDefinitions);
                                    Logger.LogInfo($"Loaded Scene Definitions {sceneDefinitions.Select(sd => sd.name).Aggregate((a, b) => $"{a}, {b}")}");
                                }

                                var campaignDefinitions = bundle.LoadAllAssets<CampaignDefinition>();
                                if (sceneDefinitions.Length > 0)
                                {
                                    Campaigns.AddRange(campaignDefinitions);
                                    Logger.LogInfo($"Created and Loaded {campaignDefinitions.Length} CampaignDefinitions from Definitions File {definitionBundle}");
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

            CurrentCampaign = Campaigns.FirstOrDefault(campaign => campaign.name == "RiskOfRain2Campaign") ?? Campaigns.First();
            selectedCampaignIndex = Campaigns.IndexOf(CurrentCampaign);

            Instance = this;

            Initialized?.Invoke(this, EventArgs.Empty);

            var mmcStart = typeof(MainMenuController).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            var sdAwake = typeof(SceneDef).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            var runPNS = typeof(Run).GetMethod(nameof(Run.PickNextStageScene));

            HookEndpointManager.Add<Hook<MainMenuController>>(mmcStart, (Hook<MainMenuController>)MainMenuController_Start);
            HookEndpointManager.Add<Hook<SceneDef>>(sdAwake, (Hook<SceneDef>)SceneDef_Awake);
            HookEndpointManager.Add<Hook<Run>>(runPNS, (Hook<Run, SceneDef[]>)(Run_PickNextStageScene));

            SceneCatalog.getAdditionalEntries += ProvideAdditionalSceneDefs;
        }

        private void Run_PickNextStageScene(MethodCall<Run, SceneDef[]> orig, Run self, SceneDef[] choices)
        {
            if (CurrentCampaign?.StartSegment != null)
                self.nextStageScene = CurrentCampaign.PickNextScene(self.nextStageRng, self);
            else
                orig(self, choices);
        }

        private void ProvideAdditionalSceneDefs(List<SceneDef> obj)
        {
            Logger.LogInfo("Loading additional scenes");
            obj.AddRange(sceneDefList);
        }

        private void SceneDef_Awake(MethodCall<SceneDef> orig, SceneDef self)
        {
            if (self is SceneDefReference sdr)
            {
                var def = Resources.Load<SceneDef>($"SceneDefs/{sdr.name}");
                foreach (var field in sceneDefFields)
                    field.SetValue(self, field.GetValue(def));
            }
            orig(self);
        }

        private void MainMenuController_Start(MethodCall<MainMenuController> orig, MainMenuController self)
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
    }
}