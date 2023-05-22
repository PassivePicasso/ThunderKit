using AssetsExporter.Extensions;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsExporter.Collection
{
    public class AssetCollection : BaseAssetCollection
    {
        private static readonly Dictionary<AssetClassID, string> typeToExtension = new Dictionary<AssetClassID, string>
        {
            [AssetClassID.GameObject] = "prefab",
            [AssetClassID.Material] = "mat",
            [AssetClassID.AnimatorController] = "controller",
            [AssetClassID.AnimatorOverrideController] = "overrideController",
            [AssetClassID.AnimationClip] = "anim",
        };
        //Here's the list of all supported extensions for NativeFormatImporter
        //Probably need to assign everything to its type id
        //It's not strictly required (aside from prefab) but may cause problems with resources file export
        //because different collections can have the same path but different extension
        //
        //anim, animset, asset, blendtree, buildreport, colors, controller, cubemap, curves, curvesNormalized, flare, fontsettings, giparams, gradients, guiskin, ht, mask, mat, mesh, mixer, overrideController, particleCurves, particleCurvesSigned, particleDoubleCurves, particleDoubleCurvesSigned, physicMaterial, physicsMaterial2D, playable, preset, renderTexture, shadervariants, spriteatlas, state, statemachine, texture2D, transition, webCamTexture, brush, terrainlayer, signal
        public override string ExportExtension => typeToExtension.TryGet((AssetClassID)(MainAsset?.info.TypeId ?? -1u), "asset");

        public static AssetCollection CreateAssetCollection(AssetsManager assetsManager, AssetExternal asset)
        {
            var collection = new AssetCollection();

            var rootAsset = AssetsHelpers.GetRootAsset(assetsManager, asset);
            collection.Assets.AddRange(AssetsHelpers.GetAssetWithSubAssets(assetsManager, rootAsset));

            return collection;
        }
    }
}
