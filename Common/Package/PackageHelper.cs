using System.IO;
using ThunderKit.Common.Configuration;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Common.Package
{
    public static class PackageHelper
    {
        public static void GeneratePackageManifest(string packageName, string outputDir, string modName, string authorAlias, string modVersion, string description = null, string url = null)
        {
            string unityVersion = Application.unityVersion.Substring(0, Application.unityVersion.LastIndexOf("."));
            var author = new Author
            {
                name = authorAlias,
                url = url
            };
            var packageManifest = new PackageManagerManifest(author, packageName, ObjectNames.NicifyVariableName(modName), modVersion, unityVersion, description);
            var packageManifestJson = JsonUtility.ToJson(packageManifest);
            ScriptingSymbolManager.AddScriptingDefine(packageName);
            File.WriteAllText(Path.Combine(outputDir, "package.json"), packageManifestJson);
        }
    }
}