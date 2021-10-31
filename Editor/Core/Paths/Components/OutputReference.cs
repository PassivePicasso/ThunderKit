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
                Errored = false;
                ErrorMessage = string.Empty;
                return reference.GetPath(pipeline);
            }
            catch (NullReferenceException nre)
            {
                Errored = true;
                ErrorMessage = nre.Message;
                ErrorStacktrace = nre.StackTrace;
                var pathReferencePath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(output));
                var pathReferenceLink = $"[{output.name}.{name}.reference](assetlink://{pathReferencePath})";
                throw new InvalidOperationException($"Error {pathReferenceLink} is unassigned or null", nre);

            }
            catch (Exception e)
            {
                Errored = true;
                ErrorMessage = e.Message;
                ErrorStacktrace = e.StackTrace;
                var pathReferencePath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(output));
                var pathReferenceLink = $"[{output.name}.{name}.reference({reference.name})](assetlink://{pathReferencePath})";
                throw new InvalidOperationException($"Error Invoking PathReference: {pathReferenceLink}", e);
            }
        }
    }
}
