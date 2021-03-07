using System.IO;
using System.Linq;
using ThunderKit.Common.Package;
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