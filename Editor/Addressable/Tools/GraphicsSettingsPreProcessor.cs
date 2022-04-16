#if TK_ADDRESSABLE
using System.Linq;

namespace ThunderKit.Addressable.Tools
{
    public class GraphicsSettingsPreProcessor : UnityEditor.AssetModificationProcessor
    {
        private const string GraphicsSettingsPath = "ProjectSettings/GraphicsSettings.asset";

        public static string[] OnWillSaveAssets(string[] paths)
        {
            if (paths.Contains(GraphicsSettingsPath))
                AddressableGraphicsSettings.UnsetAllShaders();

            return paths;
        }
    }
}
#endif