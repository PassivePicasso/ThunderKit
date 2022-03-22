using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UObject = UnityEngine.Object;

namespace ThunderKit.UNetUtil
{
    public static class AssetUtil
    {
        public static void CheckNetworkIdentities(List<string> assetPaths, Pipeline pipeline)
        {
            try
            {
                List<GameObject> prefabs = assetPaths.Select(ap => AssetDatabase.LoadAssetAtPath<UObject>(ap))
                    .Where(obj => obj is GameObject)
                    .OfType<GameObject>()
                    .Where(go => PrefabUtility.IsPartOfPrefabAsset(go))
                    .Where(go => go.GetComponent<NetworkIdentity>() && !go.GetComponent<NetworkIdentity>().assetId.IsValid())
                    .ToList();

                if(prefabs.Count > 0)
                {
                    pipeline.Log(LogLevel.Information, $"Found a total of {prefabs.Count} prefabs with invalid NetworkIdentity assetID's, fixing.",
                        prefabs.Select(go => $"* {GenerateAssetLink(go)}").ToArray());

                    List<string> logger = new List<string>();
                    foreach(GameObject networkedPrefab in prefabs)
                    {
                        try
                        {
                            logger.Add(FixPrefab(networkedPrefab));
                        }
                        catch(Exception ex)
                        {
                            pipeline.Log(LogLevel.Error, ex.ToString());
                        }
                    }

                    pipeline.Log(LogLevel.Information, $"Fixed a total of {prefabs.Count} prefabs with invalid NetworkIdentity assetID's, listing changes.", logger.ToArray());
                }

            }
            catch (Exception ex)
            {
                pipeline.Log(LogLevel.Error, ex.ToString());
            }
        }

        #region Adapted code from NetworkIdentity
        private static string FixPrefab(GameObject networkedPrefab)
        {
            NetworkIdentity identity = networkedPrefab.GetComponent<NetworkIdentity>();
            if(IsThisAPrefab(networkedPrefab))
            {
                var message = ForceSceneIDAndAssignAssetID(networkedPrefab);
                PrefabUtility.SavePrefabAsset(networkedPrefab);
                return $"* {message}";
            }
            else if(IsThisASceneObjectWithThatReferencesPrefabAsset(networkedPrefab, out GameObject prefab))
            {
                var message = AssignAssetID(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                return $"* {message}";
            }
            else
            {
                var message = ResetAssetID(networkedPrefab);
                PrefabUtility.SavePrefabAsset(networkedPrefab);
                return $"* {message}";
            }
        }

        private static string ForceSceneIDAndAssignAssetID(GameObject networkedPrefab)
        {
            NetworkIdentity identity = networkedPrefab.GetComponent<NetworkIdentity>();
            identity.ForceSceneId(0);
            string assetIDMsg = AssignAssetID(networkedPrefab);
            return $"Forced SceneID of {networkedPrefab.name} to 0 and {assetIDMsg}";
        }

        private static string AssignAssetID(GameObject networkedPrefab)
        {
            NetworkIdentity identity = networkedPrefab.GetComponent<NetworkIdentity>();

            FieldInfo field = identity.GetType().GetField("m_AssetId", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new NullReferenceException($"Could not find field \"m_AssetID\" within the network identity of {networkedPrefab}!");

            string path = AssetDatabase.GetAssetPath(networkedPrefab);
            NetworkHash128 hash = NetworkHash128.Parse(AssetDatabase.AssetPathToGUID(path));

            field.SetValue(identity, hash);

            return $"Set AssetID of {networkedPrefab.name} to \"{hash}\"";
        }

        private static string ResetAssetID(GameObject networkedPrefab)
        {
            NetworkIdentity identity = networkedPrefab.GetComponent<NetworkIdentity>();

            FieldInfo field = identity.GetType().GetField("m_AssetId", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new NullReferenceException($"Could not find field \"m_AssetID\" within the network identity of {networkedPrefab}!");

            NetworkHash128 hash = (NetworkHash128)field.GetValue(identity);
            hash.Reset();
            field.SetValue(identity, hash);

            return $"Reset AssetID of {networkedPrefab}";
        }
        private static bool IsThisAPrefab(GameObject prefabToTest) => PrefabUtility.IsPartOfPrefabAsset(prefabToTest);

        private static bool IsThisASceneObjectWithThatReferencesPrefabAsset(GameObject testedGo, out GameObject resultingGo)
        {
            resultingGo = null;
            if (!PrefabUtility.IsPartOfNonAssetPrefabInstance(testedGo))
                return false;
            resultingGo = (GameObject)PrefabUtility.GetCorrespondingObjectFromOriginalSource(testedGo);
            if(resultingGo == null)
            {
                return false;
            }
            return true;
        }
        #endregion

        private static string GenerateAssetLink(GameObject obj) => $"[{obj.name}](assetlink://{UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(obj))})";
    }
}
