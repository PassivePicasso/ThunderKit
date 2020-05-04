using RoR2;
using UnityEngine;

namespace RainOfStages.Proxy
{
    [CreateAssetMenu(menuName = "Rain of Stages/Scene Definition")]
    public class CustomSceneDefProxy : SceneDef
    {
        //public static Dictionary<SceneDefProxy, SceneDef> loadedProxies = new Dictionary<SceneDefProxy, SceneDef>();
        //public string nameToken;
        //public string subtitleToken;
        //public string loreToken;
        //public int stageOrder;
        //public Texture previewTexture;
        //public GameObject dioramaPrefab;
        //public SceneType sceneType;
        //public string songName;
        //public string bossSongName;
        //public string cachedSceneName;

        //public override SceneDef ToSceneDef()
        //{
        //    var def = ScriptableObject.CreateInstance<SceneDef>();
        //    def.bossSongName = bossSongName;
        //    def.songName = songName;
        //    def.dioramaPrefab = dioramaPrefab;
        //    def.loreToken = loreToken;
        //    def.subtitleToken = subtitleToken;
        //    def.nameToken = nameToken;
        //    def.previewTexture = previewTexture;
        //    def.sceneType = SceneType.Stage;
        //    def.destinations = new SceneDef[0];
        //    def.stageOrder = stageOrder;
        //    ((ScriptableObject)def).name = name;
        //    loadedProxies[this] = def;


        //    typeof(SceneDef).GetProperty("baseSceneName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(def, name);
        //    def.sceneNameOverrides = new List<string>();

        //    return def;
        //}
    }

}