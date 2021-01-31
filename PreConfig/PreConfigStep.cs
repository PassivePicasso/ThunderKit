using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.PreConfig
{
    [InitializeOnLoad]
    public class PreConfigStep
    {
        static PreConfigStep()
        {
            var compression = "Compression";
            var siocfs = "System.IO.Compression.FileSystem.dll";
            var sioc = "System.IO.Compression.dll";

            var packageDir = Path.Combine("Packages", compression);
            var packageJsonPath = Path.Combine(packageDir, "package.json");

            var editorPath = Path.GetDirectoryName(EditorApplication.applicationPath);
            var path = Path.Combine(editorPath, "Data", "MonoBleedingEdge", "lib", "mono", "gac");

            var destFileSystemPath = Path.Combine(packageDir, siocfs);
            var destCompressionPath = Path.Combine(packageDir, sioc);

            if (File.Exists(packageJsonPath)) return;

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

            if (!File.Exists(packageJsonPath))
            {
                File.WriteAllText(packageJsonPath, JsonUtility.ToJson(new PackageManagerManifest(
                      "system.io.compression",
                      $"System.IO.Compression",
                      "1.0.0",
                      Application.unityVersion.Substring(0, Application.unityVersion.LastIndexOf(".")),
                      $"System.IO.Compression Support"
                  )));
            }


            ScriptingSymbolManager.AddScriptingDefine("CompressionInstalled");
        }

        struct PackageManagerManifest
        {
            internal readonly static Dictionary<string, string> EmptyDictionary = new Dictionary<string, string>();

            public string name;
            public string displayName;
            public string version;
            public string unity;
            public string description;
            public Dictionary<string, string> dependencies;

            public PackageManagerManifest(string name, string displayName, string version, string unity, string description)
            {
                this.name = name;
                this.displayName = displayName;
                this.version = version;
                this.unity = unity;
                this.description = description;
                this.dependencies = EmptyDictionary;
            }
        }
    }
}