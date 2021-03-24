using ThunderKit.Installer;
using UnityEditor;

namespace ThunderKit.Common.Configuration
{
    [InitializeOnLoad]
    public class LoadCompression
    {
        static LoadCompression()
        {
            InstallThunderKit.InstallCompression();
        }
    }
}