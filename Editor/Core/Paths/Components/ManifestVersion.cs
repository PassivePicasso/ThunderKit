using System;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine.Networking;

namespace ThunderKit.Core.Paths.Components
{
    public class ManifestVersion : PathComponent
    {
        protected override string GetPathInternal(PathReference output, Pipeline pipeline)
        {
            try
            {
                return pipeline.Manifest.Identity.Version;
            }
            catch (NullReferenceException nre)
            {
                var pathReferencePath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(output));
                var pathReferenceLink = $"[{output.name}.{name}](assetlink://{pathReferencePath})";

                if (pipeline.Manifest == null)
                {
                    throw new NullReferenceException($"Error Invoking PathComponent: {pathReferenceLink}, Manifest not found", nre);
                }
                else if (pipeline.Manifest.Identity == null)
                {
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
