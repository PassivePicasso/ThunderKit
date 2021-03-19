using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif


namespace ThunderKit.Core.UIElements
{
    public class AssetLink : Label
    {

        public string AssetPath { get; set; }

        public AssetLink()
        {

        }
        public new class UxmlFactory : UxmlFactory<AssetLink, UxmlTraits> { }

        public new class UxmlTraits : Label.UxmlTraits
        {
            private UxmlStringAttributeDescription m_assetPath = new UxmlStringAttributeDescription { name = "asset-path" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var assetLink = (AssetLink)ve;
                assetLink.AssetPath = m_assetPath.GetValueFromBag(bag, cc);
                assetLink.AddToClassList("asset-link");
                assetLink.RegisterCallback<MouseUpEvent>(OnMouseUp);
            }

            private void OnMouseUp(MouseUpEvent evt)
            {
                var assetLink = evt.currentTarget as AssetLink;
                if (assetLink == null) return;
                var path = assetLink.AssetPath;
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
            }

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

    }
}
