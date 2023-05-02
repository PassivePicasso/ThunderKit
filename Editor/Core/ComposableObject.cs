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

			DeleteElementAtIndex(dataArray, index);

			DestroyImmediate(instance);

			so.SetIsDifferentCacheDirty();
			so.ApplyModifiedProperties();
		}

		private void DeleteElementAtIndex(SerializedProperty array, int index)
		{
			var elementToDelete = array.GetArrayElementAtIndex(index);
			if (elementToDelete.propertyType == SerializedPropertyType.ObjectReference)
				elementToDelete.objectReferenceValue = null;
			array.DeleteArrayElementAtIndex(index);
		}
	}
}