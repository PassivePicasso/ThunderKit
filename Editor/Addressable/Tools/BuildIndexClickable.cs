using LiteDB;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ThunderKit.Addressable.Tools
{
    [Serializable]
    public class KeyData
    {
        public string Address { get; set; }
        public string Name { get; set; }
        public string AssemblyQualifiedType { get; set; }
        public string TypeName { get; set; }
        public string TypeNamespace { get; set; }
        public string AssemblyName { get; set; }

        //public System.Type Type => System.Type.GetType(assemblyQualifiedType);
    }

    public class BuildIndexClickable : Clickable
    {
        private static string DatabasePath => Path.Combine("Library", "TK_Addressables.db");

        static void Empty() { }

        static int index = 0;
        private Button buildButton;
        private string[] keyStrings;
        private AsyncOperationHandle<Object> activeLoadOperation;

        public BuildIndexClickable() : base(Empty)
        {
            this.clicked += BuildIndex;
            buildButton = target as Button;

        }

        [InitializeOnLoadMethod]
        static void Initialize()
        {
        }


        public void BuildIndex()
        {
            if (buildButton == null)
                buildButton = target as Button;

            var firstLocator = Addressables.ResourceLocators.First();
            keyStrings = firstLocator.Keys.Select(key => $"{key}").Where(key => !key.EndsWith(".bundle")).ToArray();
            index = 0;
            buildButton.SetEnabled(false);
            EditorApplication.update += OnIndex;
        }

        private void OnIndex()
        {
            try
            {
                if (index >= keyStrings.Length - 1 || (activeLoadOperation.IsValid() && !activeLoadOperation.IsDone))
                    return;

                var firstKey = keyStrings[index];
                activeLoadOperation = Addressables.LoadAssetAsync<Object>(firstKey);

                activeLoadOperation.Completed += ActiveLoadOperation_Completed;

                void ActiveLoadOperation_Completed(AsyncOperationHandle<Object> obj)
                {
                    var result = activeLoadOperation.Result;
                    if (result == null)
                        return;

                    var resultType = result.GetType();

                    var data = new KeyData
                    {
                        Address = firstKey.ToString(),
                        Name = result.name,
                        AssemblyQualifiedType = resultType.AssemblyQualifiedName,
                        TypeName = resultType.Name,
                        TypeNamespace = resultType.Namespace,
                        AssemblyName = resultType.Assembly.FullName
                    };

                    using (var AddressableDatabase = new LiteDatabase(DatabasePath))
                    {
                        var collection = AddressableDatabase.GetCollection<KeyData>("AddressData");
                        collection.Insert(data);
                    }
                    //AddressableDatabase.EnsureIndex(kd => kd.address);
                    //AddressableDatabase.EnsureIndex(kd => kd.name);
                    //AddressableDatabase.EnsureIndex(kd => kd.typeName);
                    //AddressableDatabase.EnsureIndex(kd => kd.typeNamespace);
                    //AddressableDatabase.EnsureIndex(kd => kd.assemblyName);
                 }
                //Addressables.Release(assetOp);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                index++;

                if (index >= keyStrings.Length - 1)
                {
                    EditorApplication.update -= OnIndex;
                    buildButton.SetEnabled(true);
                }
            }
        }

    }
}