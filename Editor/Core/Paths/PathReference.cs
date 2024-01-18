using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ThunderKit.Common;
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine.Networking;
using static System.IO.Path;

namespace ThunderKit.Core.Paths
{
    public class PathReference : ComposableObject
    {
        [MenuItem(Constants.ThunderKitContextRoot + nameof(PathReference), false, priority = Constants.ThunderKitMenuPriority)]
        public static void Create() => ScriptableHelper.SelectNewAsset<PathReference>();

        const string pathReferenceCacheKey = "PathReferenceCache";
        const char opo = '<';
        const char opc = '>';
        private static readonly Regex referenceIdentifier = new Regex($"\\{opo}(.*?)\\{opc}", RegexOptions.Compiled);

        public static string ResolvePath(string input, Pipeline pipeline, UnityEngine.Object caller)
        {
            var result = input;

            Dictionary<string, PathReference> pathReferenceCache;
            if (!pipeline || pipeline.ExecutionInfo == null)
            {
                pathReferenceCache = FindAllPathReferences();
            }
            else if (!pipeline.ExecutionInfo.TryGetValue(pathReferenceCacheKey, out pathReferenceCache))
            {
                pipeline.ExecutionInfo[pathReferenceCacheKey] = pathReferenceCache = FindAllPathReferences(); 
            }

            var callerPath = string.Empty;
            var callerLink = string.Empty;

            if (pipeline != null && caller)
            {
                callerPath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(caller));
                callerLink = $"[{pipeline.name}.{caller.name}](assetlink://{callerPath})";
            }

            var match = referenceIdentifier.Match(result);
            while (match != null && !string.IsNullOrEmpty(match.Value))
            {
                var matchValue = match.Value.Trim(opo, opc);
                if (!pathReferenceCache.TryGetValue(matchValue, out var pathReference))
                {
                    if (caller)
                    {
                        EditorGUIUtility.PingObject(caller);
                    }
                    throw new InvalidOperationException($"{callerLink} No PathReference named \"{matchValue}\" found in AssetDatabase");
                }

                var replacement = pathReference.GetPath(pipeline);
                result = result.Replace(match.Value, replacement);
                match = match.NextMatch();
            }

            return result.Replace("\\", "/");
        }

        public override Type ElementType => typeof(PathComponent);

        public override bool SupportsType(Type type) => ElementType.IsAssignableFrom(type);

        public string GetPath(Pipeline pipeline)
        {
            return Combine(Data.OfType<PathComponent>().Select(pc => pc.GetPath(this, pipeline)).ToArray());
        }

        private static Dictionary<string, PathReference> FindAllPathReferences()
        {
            var pathReferenceGuids = AssetDatabase.FindAssets($"t:{nameof(PathReference)}", Constants.FindAllFolders);
            return pathReferenceGuids
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Select(x => AssetDatabase.LoadAssetAtPath<PathReference>(x))
                .Where(x => x != null)
                .ToDictionary(pr => pr.name);
        }

        public override string ElementTemplate =>
$@"using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Paths;

namespace {{0}}
{{{{
    public class {{1}} : PathComponent
    {{{{
        public override string GetPath({nameof(PathReference)} output, Pipeline pipeline)
        {{{{
            return base.GetPath(output, pipeline);
        }}}}
    }}}}
}}}}
";
    }
}
