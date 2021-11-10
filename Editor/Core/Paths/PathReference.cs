using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ThunderKit.Common;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine.Networking;
using static System.IO.Path;

namespace ThunderKit.Core.Paths
{
    public class PathReference : ComposableObject
    {
        [MenuItem(Constants.ThunderKitContextRoot + nameof(PathReference), false, priority = Constants.ThunderKitMenuPriority)]
        public static void Create() => ScriptableHelper.SelectNewAsset<PathReference>();

        const char opo = '<';
        const char opc = '>';
        private static readonly Regex referenceIdentifier = new Regex($"\\{opo}(.*?)\\{opc}", RegexOptions.Compiled);
        public static string ResolvePath(string input, Pipeline pipeline, UnityEngine.Object caller)
        {
            var result = input;
            var pathReferenceGuids = AssetDatabase.FindAssets($"t:{nameof(PathReference)}", Constants.AssetDatabaseFindFolders);
            var pathReferencePaths = pathReferenceGuids.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToArray();
            var pathReferences = pathReferencePaths.Select(x => AssetDatabase.LoadAssetAtPath<PathReference>(x)).ToArray();
            var pathReferenceDictionary = pathReferences.ToDictionary(pr => pr.name);

            var callerPath = string.Empty;
            var callerLink = string.Empty;

            if (pipeline != null && caller)
            {
                callerPath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(caller));
                callerLink = $"[{pipeline.name}.{caller.name}](assetlink://{callerPath})";
            }

            Match match = referenceIdentifier.Match(result);
            while (match != null && !string.IsNullOrEmpty(match.Value))
            {
                var matchValue = match.Value.Trim(opo, opc);
                if (!pathReferenceDictionary.ContainsKey(matchValue))
                {
                    if (caller)
                    {
                        EditorGUIUtility.PingObject(caller);
                    }
                    throw new InvalidOperationException($"{callerLink} No PathReference named \"{matchValue}\" found in AssetDatabase");
                }

                var pathReference = pathReferenceDictionary[matchValue];
                var pathReferencePath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(pathReference));
                var pathReferenceLink = $"[{pathReference.name}](assetlink://{pathReferencePath})";

                var replacement = pathReference.GetPath(pipeline);
                if (replacement == null) throw new NullReferenceException($"{callerLink} {pathReferenceLink} returned null. Error may have been encountered");
                result = result.Replace(match.Value, replacement);
                match = match.NextMatch();
            }

            return result.Replace("\\", "/");
        }

        public override Type ElementType => typeof(PathComponent);

        public override bool SupportsType(Type type) => ElementType.IsAssignableFrom(type);

        public string GetPath(Pipeline pipeline)
        {
            string result = string.Empty;
            foreach (var pc in Data.OfType<PathComponent>())
            {
                //try
                //{
                    result = Combine(result, pc.GetPath(this, pipeline));
                //}
                //catch (Exception e)
                //{
                //    var pathReferencePath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(this));
                //    var pathReferenceLink = $"[{name}](assetlink://{pathReferencePath})";

                //    var exception = new InvalidOperationException($"{pipeline.pipelineLink} {pathReferenceLink} resolution failed", e);
                //    throw exception;
                //}
            }
            return result;
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
