using System.IO;
using System.Linq;
using ThunderKit.Common.Package;
using UnityEditor;

namespace ThunderKit.Common.Configuration
{
    [InitializeOnLoad]
    public class LoadCompression
    {
        static LoadCompression()
        {
            var compression = "Compression";
            var siocfs = "System.IO.Compression.FileSystem.dll";
            var sioc = "System.IO.Compression.dll";

            var packageDir = Path.Combine("Packages", compression);

            var editorPath = Path.GetDirectoryName(EditorApplication.applicationPath);
            var path = Path.Combine(editorPath, "Data", "MonoBleedingEdge", "lib", "mono", "gac");

            var destFileSystemPath = Path.Combine(packageDir, siocfs);
            var destCompressionPath = Path.Combine(packageDir, sioc);

            if (!Directory.Exists(packageDir)) Directory.CreateDirectory(packageDir);

            if (!File.Exists(destCompressionPath))
            {
                var compressionDll = Directory.EnumerateFiles(path, sioc, SearchOption.AllDirectories).FirstOrDefault();
                File.Copy(compressionDll, destCompressionPath, true);
            }

            if (!File.Exists(destFileSystemPath))
            {
                var fileSystemDll = Directory.EnumerateFiles(path, siocfs, SearchOption.AllDirectories).FirstOrDefault();
                File.Copy(fileSystemDll, destFileSystemPath, true);
            }

            PackageHelper.GeneratePackageManifest(
                "system.io.compression", packageDir,
                "System.IO.Compression", "Microsoft",
                "1.0.0",
                "System.IO.Compression and System.IO.Compression.FileSystem");
        }
    }
}