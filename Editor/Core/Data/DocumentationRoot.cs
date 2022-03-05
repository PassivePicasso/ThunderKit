using ThunderKit.Common;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Data
{
    using static ScriptableHelper;
    public class DocumentationRoot : ScriptableObject
    {
        [MenuItem(Constants.ThunderKitContextRoot + nameof(DocumentationRoot), false, priority = Constants.ThunderKitMenuPriority)]
        public static void Create()
        {
            SelectNewAsset<DocumentationRoot>();
        }

        public TextAsset MainPage;
    }
}