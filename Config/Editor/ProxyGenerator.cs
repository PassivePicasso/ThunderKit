#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Utilities;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Config
{
    public static class ProxyGenerator
    {
        [MenuItem("ThunderKit/Generate Proxies")]
        public static void GenerateProxies()
        {
            var settings = ThunderKitSettings.GetOrCreateSettings();
            var currentDir = Directory.GetCurrentDirectory();
            var proxyPath = Path.Combine(currentDir, "Assets", Path.GetFileNameWithoutExtension(settings.GameExecutable), "GeneratedProxies");
            var assembliesPath = Path.Combine(currentDir, "Assets", "Assemblies");
            var gameAssembly = EditorUtility.OpenFilePanel("Open Game Assembly", assembliesPath, "dll");
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.CodeBase.Contains(gameAssembly));

            if (assembly == null)
            {
                Debug.LogError("Assembly not loaded cannot generate proxies. Only load assemblies from under the Assets directory.");
                return;
            }

            var assemblyTypes = assembly.GetTypes();
            var uniObjects = assemblyTypes.Where(t => typeof(UnityEngine.Object).IsAssignableFrom(t));
            var nonSealed = uniObjects.Where(t => !t.IsSealed);
            var nonAbstract = nonSealed.Where(t => !t.IsAbstract);
            var nonPrivate = nonAbstract.Where(t => t.IsPublic);
            var targetTypes = nonPrivate.Where(t => t.IsClass);

            if (!Directory.Exists(proxyPath)) Directory.CreateDirectory(proxyPath);
            foreach (var type in targetTypes)
            {
                var isGlobal = string.IsNullOrWhiteSpace(type.Namespace);
                var namespaceSeparator = isGlobal ? "" : ".";
                var definition = string.Join(Environment.NewLine,
                        isGlobal ? "" : $"using global::{type.Namespace};",
                        $"namespace {nameof(PassivePicasso)}.{nameof(ThunderKit)}.Proxy{namespaceSeparator}{type.Namespace}",
                        $"{{",
                        $"    public class {type.Name} : global::{type.FullName} {{}}",
                        $"}}"
                    );

                var filePath = proxyPath;
                if (!isGlobal) filePath = Path.Combine(proxyPath, Path.Combine(type.Namespace.Split('.')));

                string fileName = $"{type.Name}.cs";
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);

                filePath = Path.Combine(filePath, fileName);
                if (!File.Exists(filePath)) File.Delete(filePath);

                File.WriteAllText(filePath, definition);
            }

            AssetDatabase.Refresh();
        }
    }
}
#endif