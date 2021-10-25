using System;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine.Networking;

namespace ThunderKit.Core.Paths.Components
{
    public class ManifestName : PathComponent
    {
        protected override string GetPathInternal(PathReference output, Pipeline pipeline)
        {
            try
            {
                return pipeline.Manifest.Identity.Name;
            }
            catch (NullReferenceException nre)
            {
                var pathReferencePath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(output));
                var pathReferenceLink = $"[{output.name}.{name}](assetlink://{pathReferencePath})";

                if (pipeline.Manifest == null)
                {
                    Errored = true;
                    ErrorMessage = $"Manifest not found";
                    ErrorStacktrace = nre.StackTrace;
                    throw new NullReferenceException($"Error Invoking PathComponent: {pathReferenceLink}, Manifest not found", nre);
                }
                else if (pipeline.Manifest.Identity == null)
                {
                    Errored = true;
                    ErrorMessage = $"ManifestIdentity not found";
                    ErrorStacktrace = nre.StackTrace;
                    throw new NullReferenceException($"Error Invoking PathComponent: {pathReferenceLink}, ManifestIdentity not found", nre);
                }
                throw;
            }
            catch
            {
                throw;
            }
        }
    }
}
