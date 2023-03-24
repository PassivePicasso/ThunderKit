using AssetsExporter.Extensions;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsExporter
{
    public static class AssetsHelpers
    {
        public static AssetExternal GetRootAsset(AssetsManager assetsManager, AssetExternal asset)
        {
            var baseField = asset.baseField;

            if (baseField.TypeName == "GameObject")
            {
                return GetRootGameObject(assetsManager, asset);
            }
            
            if (baseField.TypeName == "Sprite")
            {
                var firstExt = assetsManager.GetExtAsset(asset.file, baseField.Get("m_RD").Get("texture"));
                if (firstExt.info != null)
                {
                    return firstExt;
                }
                return asset;
            }

            if (baseField.TemplateField.Children.Any(f => f.Name == "m_GameObject"))
            {
                var firstExt = assetsManager.GetExtAsset(asset.file, baseField.Get("m_GameObject"));
                if (firstExt.info != null)
                {
                    return GetRootGameObject(assetsManager, firstExt);
                }
                return asset;
            }

            return asset;
        }

        private static AssetExternal GetRootGameObject(AssetsManager assetsManager, AssetExternal ext)
        {
            var transformExt = assetsManager.GetExtAsset(ext.file, ext.baseField.Get("m_Component").Get("Array")[0].GetLastChild());
            while (true)
            {
                var parentExt = assetsManager.GetExtAsset(ext.file, transformExt.baseField.Get("m_Father"));
                if (parentExt.baseField == null)
                {
                    var gameObjectExt = assetsManager.GetExtAsset(ext.file, transformExt.baseField.Get("m_GameObject"));
                    return gameObjectExt;
                }
                transformExt = parentExt;
            }
        }

        public static IEnumerable<AssetExternal> GetAssetWithSubAssets(AssetsManager assetsManager, AssetExternal rootAsset)
        {
            var baseField = rootAsset.baseField;
            if (baseField.TypeName == "GameObject")
            {
                return GatherAllGameObjectsAssets(assetsManager, rootAsset);
            }
            if (baseField.TypeName.StartsWith("Texture"))
            {
                return GatherAllTextureAssets(assetsManager, rootAsset);
            }
#warning TODO: AnimatorController also has subAssets, such as AnimatorState, AnimatorStateMachine and BlendTree???
#warning TODO: TimelineAsset can have AnimationClips ("m_Tracks"), but it's also just a scriptable object and not a special type {fileID: 337831424, guid: 6a10b2909283487f913b00d94cd3faf5, type: 3}
            return new[] { rootAsset };
        }

        private static IEnumerable<AssetExternal> GatherAllTextureAssets(AssetsManager assetsManager, AssetExternal rootAsset)
        {
            var file = rootAsset.file;

            yield return rootAsset;

            foreach (var spriteAsset in file.file.GetAssetsOfType(AssetClassID.Sprite))
            {
                var spriteExt = assetsManager.GetExtAsset(file, 0, spriteAsset.PathId);
                var textureExt = GetRootAsset(assetsManager, spriteExt);
                if (rootAsset.info.PathId == textureExt.info.PathId)
                {
                    yield return spriteExt;
                }
            }
        }

        private static IEnumerable<AssetExternal> GatherAllGameObjectsAssets(AssetsManager assetsManager, AssetExternal rootAsset)
        {
            var baseField = rootAsset.baseField;
            var file = rootAsset.file;

            yield return rootAsset;

            var components = baseField.Get("m_Component").Get("Array");

            foreach (var component in components.Children)
            {
                var componentExternal = assetsManager.GetExtAsset(file, component.GetLastChild());
                yield return componentExternal;
            }

            var children = assetsManager
                .GetExtAsset(file, baseField.Get("m_Component").Get("Array")[0].GetLastChild())
                .baseField
                .Get("m_Children")
                .Get("Array");


            for (int i = 0; i < children.Children.Count; i++)
            {
                var childExternal = assetsManager.GetExtAsset(file, children[i]);
                var gameObjExt = assetsManager.GetExtAsset(file, childExternal.baseField.Get("m_GameObject"));

                foreach (var subAsset in GatherAllGameObjectsAssets(assetsManager, gameObjExt))
                {
                    yield return subAsset;
                }
            }
        }
    }
}
