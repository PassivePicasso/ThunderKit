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
                // Use root manifest when no specific manifest context (ManifestIndex = -1)
                var targetManifest = (pipeline.ManifestIndex >= 0 && pipeline.ManifestIndex < pipeline.Manifests?.Length)
                    ? pipeline.Manifest
                    : pipeline.manifest;

                return targetManifest?.Identity?.Version;
            }
            catch (Exception ex) when (!(ex is IndexOutOfRangeException))
            {
                var pathReferencePath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(output));
                var pathReferenceLink = $"[{output.name}.{name}](assetlink://{pathReferencePath})";

                var targetManifest = (pipeline.ManifestIndex >= 0 && pipeline.ManifestIndex < pipeline.Manifests?.Length)
                    ? pipeline.Manifest
                    : pipeline.manifest;

                if (targetManifest == null)
                {
                    throw new NullReferenceException($"Error Invoking PathComponent: {pathReferenceLink}, Manifest not found", ex);
                }
                else if (targetManifest.Identity == null)
                {
                    throw new NullReferenceException($"Error Invoking PathComponent: {pathReferenceLink}, ManifestIdentity not found", ex);
                }
                throw;
            }
        }
    }
}
