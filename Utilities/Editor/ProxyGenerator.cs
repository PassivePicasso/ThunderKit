using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace PassivePicasso.ThunderKit.Utilities
{
    public class ProxyGenerator
    {
        public static void GenerateProxies(Type targetType, string gameDir)
        {
            var currentDir = Directory.GetCurrentDirectory();
            string proxyPath = Path.Combine(currentDir, "RainOfStages", gameDir, "GeneratedProxies");

            var assembly = targetType.Assembly;

            var assemblyTypes = assembly.GetTypes();
            var uniObjects = assemblyTypes.Where(t => typeof(UnityEngine.Object).IsAssignableFrom(t));
            var nonSealed = uniObjects.Where(t => !t.IsSealed);
            var nonAbstract = nonSealed.Where(t => !t.IsAbstract);
            var nonPrivate = nonAbstract.Where(t => t.IsPublic);
            var targetTypes = nonPrivate.Where(t => t.IsClass);

            if (!Directory.Exists(proxyPath))
                Directory.CreateDirectory(proxyPath);

            foreach (var type in targetTypes)
            {
                var isGlobal = string.IsNullOrWhiteSpace(type.Namespace);
                var namespaceSeparator = isGlobal ? "" : ".";
                var definition = string.Join(Environment.NewLine,
                        isGlobal ? "" : $"using global::{type.Namespace};",
                        $"namespace {nameof(PassivePicasso)}.Proxy{namespaceSeparator}{type.Namespace}",
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