using System;
using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine.Networking;

namespace ThunderKit.Core.Paths.Components
{
    public class FindFile : PathComponent
    {
        public SearchOption searchOption;
        public string searchPattern;
        [PathReferenceResolver]
        public string path;
        protected override string GetPathInternal(PathReference output, Pipeline pipeline)
        {
            try
            {
                string resolvedPath = PathReference.ResolvePath(path, pipeline, this);

                string first = Directory.EnumerateFiles(resolvedPath, searchPattern, searchOption).First();
                return first;
            }
            catch (Exception e)
            {
                var pathReferencePath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(output));
                var pathReferenceLink = $"[{output.name}.{name}:](assetlink://{pathReferencePath})";
                throw new InvalidOperationException($"{pipeline.pipelineLink} {pathReferenceLink} {e.Message}");
            }
        }
    }
}
