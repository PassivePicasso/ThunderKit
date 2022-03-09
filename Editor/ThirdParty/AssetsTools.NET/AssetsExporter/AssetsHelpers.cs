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
            var baseField = asset.instance.GetBaseField();

            if (baseField.templateField.type == "GameObject")
            {
                return GetRootGameObject(assetsManager, asset);
            }
            
            if (baseField.templateField.type == "Sprite")
            {
                var firstExt = assetsManager.GetExtAsset(asset.file, baseField.Get("m_RD").Get("texture"));
                if (firstExt.info != null)
                {
                    return firstExt;
                }
                return asset;
            }

            if (baseField.templateField.children.Any(f => f.name == "m_GameObject"))
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
            var transformExt = assetsManager.GetExtAsset(ext.file, ext.instance.GetBaseField().Get("m_Component").Get("Array")[0].GetLastChild());
            while (true)
            {
                var parentExt = assetsManager.GetExtAsset(ext.file, transformExt.instance.GetBaseField().Get("m_Father"));
                if (parentExt.instance == null)
                {
                    var gameObjectExt = assetsManager.GetExtAsset(ext.file, transformExt.instance.GetBaseField().Get("m_GameObject"));
                    return gameObjectExt;
                }
                transformExt = parentExt;
            }
        }

        public static IEnumerable<AssetExternal> GetAssetWithSubAssets(AssetsManager assetsManager, AssetExternal rootAsset)
        {
            var baseField = rootAsset.instance.GetBaseField();
            if (baseField.templateField.type == "GameObject")
            {
                return GatherAllGameObjectsAssets(assetsManager, rootAsset);
            }
            if (baseField.templateField.type.StartsWith("Texture"))
            {
                return GatherAllTextureAssets(assetsManager, rootAsset);
            }
            return new[] { rootAsset };
        }

        private static IEnumerable<AssetExternal> GatherAllTextureAssets(AssetsManager assetsManager, AssetExternal rootAsset)
        {
            var file = rootAsset.file;

            yield return rootAsset;

            foreach (var spriteAsset in file.table.GetAssetsOfType((int)AssetClassID.Sprite))
            {
                var spriteExt = assetsManager.GetExtAsset(file, 0, spriteAsset.index);
                var textureExt = GetRootAsset(assetsManager, spriteExt);
                if (rootAsset.info.index == textureExt.info.index)
                {
                    yield return spriteExt;
                }
            }
        }

        private static IEnumerable<AssetExternal> GatherAllGameObjectsAssets(AssetsManager assetsManager, AssetExternal rootAsset)
        {
            var baseField = rootAsset.instance.GetBaseField();
            var file = rootAsset.file;

            yield return rootAsset;

            var components = baseField.Get("m_Component").Get("Array");

            foreach (var component in components.children)
            {
                var componentExternal = assetsManager.GetExtAsset(file, component.GetLastChild());
                yield return componentExternal;
            }

            var children = assetsManager
                .GetExtAsset(file, baseField.Get("m_Component").Get("Array")[0].GetLastChild())
                .instance
                .GetBaseField()
                .Get("m_Children")
                .Get("Array");


            for (int i = 0; i < children.childrenCount; i++)
            {
                var childExternal = assetsManager.GetExtAsset(file, children[i]);
                var gameObjExt = assetsManager.GetExtAsset(file, childExternal.instance.GetBaseField().Get("m_GameObject"));

                foreach (var subAsset in GatherAllGameObjectsAssets(assetsManager, gameObjExt))
                {
                    yield return subAsset;
                }
            }
        }
    }
}
