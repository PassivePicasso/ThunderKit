using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RainOfStages.Proxies
{
    [CreateAssetMenu(menuName = "ROR2/CustomSceneDefProxy")]
    public class CustomSceneDefProxy : SceneDefProxy
    {
        public static Dictionary<SceneDefProxy, SceneDef> loadedProxies = new Dictionary<SceneDefProxy, SceneDef>();
        public string nameToken;
        public string subtitleToken;
        public string loreToken;
        public int stageOrder;
        //public Texture previewTexture;
        //public GameObject dioramaPrefab;
        //public string sceneType;
        public string songName;
        public string bossSongName;
        public string cachedSceneName;

        public SceneDefProxy[] destinations;

        public override SceneDef ToSceneDef()
        {

            var def = ScriptableObject.CreateInstance<SceneDef>();
            def.bossSongName = bossSongName;
            def.songName = songName;
            //def.dioramaPrefab = proxy.dioramaPrefab;
            def.loreToken = loreToken;
            def.subtitleToken = subtitleToken;
            def.nameToken = nameToken;
            //def.previewTexture = proxy.previewTexture;
            def.sceneType = SceneType.Stage;

            def.stageOrder = stageOrder;
            ((ScriptableObject)def).name = name;
            loadedProxies[this] = def;


            typeof(SceneDef).GetProperty("sceneName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(def, name);
            foreach (var destination in destinations)
            {
                if (loadedProxies.ContainsKey(destination)) continue;
                else
                    loadedProxies[destination] = destination.ToSceneDef();
            }
            def.destinations = loadedProxies.Values.ToArray();

            //Debug.Log($"Creating SceneDef from Proxy: {name}");
            //Debug.Log($"SceneDef.sceneName: {def.sceneName}");
            //Debug.Log($"SceneDef.name: {def.name}");
            //Debug.Log($"(ScriptableObject)SceneDef.name: {((ScriptableObject)def).name}");

            return def;
        }
    }

}