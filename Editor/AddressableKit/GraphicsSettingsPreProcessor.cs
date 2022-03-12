using System.Linq;

namespace ThunderKit.RemoteAddressables
{
    public class GraphicsSettingsPreProcessor : AssetModificationProcessor
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