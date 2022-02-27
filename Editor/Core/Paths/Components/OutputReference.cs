using System;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine.Networking;

namespace ThunderKit.Core.Paths.Components
{
    public class OutputReference : PathComponent
    {
        public PathReference reference;
        protected override string GetPathInternal(PathReference output, Pipeline pipeline)
        {
            try
            {
                return reference.GetPath(pipeline);
            }
            catch (NullReferenceException nre)
            {
                var pathReferencePath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(output));
                var pathReferenceLink = $"[{output.name}.{name}.reference](assetlink://{pathReferencePath})";
                throw new InvalidOperationException($"Error {pathReferenceLink} is unassigned or null", nre);
            }
            catch (Exception e)
            {
                var pathReferencePath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(output));
                var pathReferenceLink = $"[{output.name}.{name}.reference({reference.name})](assetlink://{pathReferencePath})";
                throw new InvalidOperationException($"Error Invoking PathReference: {pathReferenceLink}", e);
            }
        }
    }
}
