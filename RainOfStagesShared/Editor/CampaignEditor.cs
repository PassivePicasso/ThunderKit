//using RainOfStages.Campaign;
//using System.Linq;
//using UnityEditor;
//using UnityEditor.Experimental.UIElements;
//using UnityEngine;
//using UnityEngine.Experimental.UIElements;
//using VisualTemplates;

//namespace RainOfStages.Editor
//{
//    public class CampaignEditor : EditorWindow
//    {
//        private static CampaignEditor window;
//        private static VisualElement rootElement;
//        private static ItemsControl listControl;

//        [SerializeField]
//        public CampaignDefinition[] campaigns;

//        [MenuItem("Window/Rain of Stages")]
//        static void LaunchEditor()
//        {
//            if (window == null || rootElement == null)
//            {
//                window = GetWindow<CampaignEditor>();
//                rootElement = window.GetRootVisualContainer();

//                window.title = "Rain of Stages";

//                rootElement.Clear();
//                listControl = new ItemsControl { bindingPath = nameof(campaigns), userData = window };

//                rootElement.Add(listControl);
//            }
//            string[] guids = AssetDatabase.FindAssets($"t:{nameof(CampaignDefinition)}");
//            string[] paths = guids.Select(path => AssetDatabase.GUIDToAssetPath(path)).ToArray();
//            window.campaigns = paths.Select(path => AssetDatabase.LoadAssetAtPath<CampaignDefinition>(path)).ToArray();
//            //listControl.value = window.campaigns;
//            rootElement.Bind(new SerializedObject(window));
//        }
//    }
//}