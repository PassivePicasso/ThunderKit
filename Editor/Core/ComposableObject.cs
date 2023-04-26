using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using System.IO;

namespace ThunderKit.Core
{
    public abstract class ComposableObject : ScriptableObject
    {
        [FormerlySerializedAs("runSteps")]
        public ComposableElement[] Data;

        public abstract bool SupportsType(Type type);

        public abstract Type ElementType { get; }

        public abstract string ElementTemplate { get; }

        public void InsertElement(ComposableElement instance, int index)
        {
            if (!ElementType.IsAssignableFrom(instance.GetType())) return;
            if (!SupportsType(instance.GetType())) return;

            AssetDatabase.AddObjectToAsset(instance, this);

            var so = new SerializedObject(this);
            var dataArray = so.FindProperty(nameof(Data));

            dataArray.InsertArrayElementAtIndex(index);
            var stepField = dataArray.GetArrayElementAtIndex(index);
            stepField.objectReferenceValue = instance;
            stepField.serializedObject.SetIsDifferentCacheDirty();
            stepField.serializedObject.ApplyModifiedProperties();
        }

        public void RemoveElement(ComposableElement instance, int index)
        {
            var so = new SerializedObject(this);
            var dataArray = so.FindProperty(nameof(Data));
            var elementAtIndex = dataArray.GetArrayElementAtIndex(index).objectReferenceValue as ComposableElement;
            if (elementAtIndex != instance)
            {
                Debug.LogError("ComposableObject.RemoveElement: instance does not match index");
                return;
            }
            AssetDatabase.RemoveObjectFromAsset(instance);

            dataArray.DeleteArrayElementAtIndex(index);

            DestroyImmediate(instance);

            for (int x = index; x < dataArray.arraySize; x++)
                dataArray.MoveArrayElement(x + 1, x);

            so.SetIsDifferentCacheDirty();
            so.ApplyModifiedProperties();
        }

		/// <summary>
		/// Function used to remove a sub-asset that is missing the script reference
		/// </summary>
		/// <param name="toDelete">The main asset that holds the sub-asset</param>
		//https://gitlab.com/RotaryHeart-UnityShare/subassetmissingscriptdelete/-/tree/master/
		public static void FixMissingScriptSubAssets(UnityEngine.Object toDelete)
		{
			//Create a new instance of the object to delete
			ScriptableObject newInstance = CreateInstance(toDelete.GetType());

			//Copy the original content to the new instance
			EditorUtility.CopySerialized(toDelete, newInstance);
			newInstance.name = toDelete.name;

			string toDeletePath = AssetDatabase.GetAssetPath(toDelete);
			string clonePath = toDeletePath.Replace(".asset", "CLONE.asset");

			//Create the new asset on the project files
			AssetDatabase.CreateAsset(newInstance, clonePath);
			AssetDatabase.ImportAsset(clonePath);

			//Unhide sub-assets
			var subAssets = AssetDatabase.LoadAllAssetsAtPath(toDeletePath);
			HideFlags[] flags = new HideFlags[subAssets.Length];
			for (int i = 0; i < subAssets.Length; i++)
			{
				//Ignore the "corrupt" one
				if (subAssets[i] == null)
					continue;

				//Store the previous hide flag
				flags[i] = subAssets[i].hideFlags;
				subAssets[i].hideFlags = HideFlags.None;
				EditorUtility.SetDirty(subAssets[i]);
			}

			EditorUtility.SetDirty(toDelete);
			AssetDatabase.SaveAssets();

			//Reparent the subAssets to the new instance
			foreach (var subAsset in AssetDatabase.LoadAllAssetRepresentationsAtPath(toDeletePath))
			{
				//Ignore the "corrupt" one
				if (subAsset == null)
					continue;

				//We need to remove the parent before setting a new one
				AssetDatabase.RemoveObjectFromAsset(subAsset);
				AssetDatabase.AddObjectToAsset(subAsset, newInstance);
			}

			//Import both assets back to unity
			AssetDatabase.ImportAsset(toDeletePath);
			AssetDatabase.ImportAsset(clonePath);

			//Reset sub-asset flags
			for (int i = 0; i < subAssets.Length; i++)
			{
				//Ignore the "corrupt" one
				if (subAssets[i] == null)
					continue;

				subAssets[i].hideFlags = flags[i];
				EditorUtility.SetDirty(subAssets[i]);
			}

			EditorUtility.SetDirty(newInstance);
			AssetDatabase.SaveAssets();

			//Here's the magic. First, we need the system path of the assets
			string globalToDeletePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), toDeletePath);
			string globalClonePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), clonePath);

			//We need to delete the original file (the one with the missing script asset)
			//Rename the clone to the original file and finally
			//Delete the meta file from the clone since it no longer exists

			File.Delete(globalToDeletePath);
			File.Delete(globalClonePath + ".meta");
			File.Move(globalClonePath, globalToDeletePath);

			AssetDatabase.Refresh();
		}

	}
}