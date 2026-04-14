using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif


namespace ThunderKit.Core.UIElements
{
#if UNITY_6000_3_OR_NEWER
    [UxmlElement]
    public partial class AssetLink : Label
#else
    public class AssetLink : Label
#endif
    {
#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute("select-asset")]
#endif
        public bool SelectAsset { get; set; } = true;
#if UNITY_6000_3_OR_NEWER
        [UxmlAttribute("asset-path")]
#endif
        public string AssetPath { get; set; }

        public AssetLink()
        {
            AddToClassList("asset-link");
            UnregisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }
        private void OnMouseUp(MouseUpEvent evt)
        {
            var assetLink = evt.currentTarget as AssetLink;
            if (assetLink == null) return;
            var path = assetLink.AssetPath;
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            EditorGUIUtility.PingObject(asset);
            if (SelectAsset)
                Selection.activeObject = asset;
        }

#if !UNITY_6000_3_OR_NEWER
        public new class UxmlFactory : UxmlFactory<AssetLink, UxmlTraits> { }

        public new class UxmlTraits : Label.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription m_assetPath = new UxmlStringAttributeDescription { name = "asset-path" };
            private readonly UxmlBoolAttributeDescription m_selectAsset = new UxmlBoolAttributeDescription { name = "select-asset" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var assetLink = (AssetLink)ve;
                assetLink.AssetPath = m_assetPath.GetValueFromBag(bag, cc);
                assetLink.SelectAsset = m_selectAsset.GetValueFromBag(bag, cc);
            }

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription =>
                Enumerable.Empty<UxmlChildElementDescription>();
        }
#endif

    }
}
