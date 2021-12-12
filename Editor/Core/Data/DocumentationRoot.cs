using ThunderKit.Common;
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
    }
}