using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Editor;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine;
using static System.IO.Path;

namespace ThunderKit.Core.Paths
{
    public class PathReference : ComposableObject, ISerializationCallbackReceiver
    {
        [MenuItem(Constants.ThunderKitContextRoot + nameof(PathReference), false, priority = Constants.ThunderKitMenuPriority)]
        public static void CreateOutput() => ScriptableHelper.SelectNewAsset<PathReference>();

        private static Dictionary<string, PathReference> referenceLookup;
        public static IReadOnlyDictionary<string, PathReference> ReferenceLookup => referenceLookup;

        private static Dictionary<Type, string[]> typeFields;
        private static Dictionary<Type, List<UnityEngine.ScriptableObject>> typeAssets;

        [InitializeOnLoadMethod]
        static void InitializeLookup()
        {
            if (referenceLookup == null)
                referenceLookup = new Dictionary<string, PathReference>();

            if (typeFields == null)
            {
                typeFields = AppDomain.CurrentDomain.GetAssemblies()
                  .SelectMany(asm => asm.GetTypes())
                  .Where(typeof(ScriptableObject).IsAssignableFrom)
                  .Select(type => (type, fields: GetTypeFields(type)))
                  .Where(proj => proj.fields.Any())
                  .ToDictionary(proj => proj.type, proj => proj.fields.Select(f => f.Name).ToArray());
            }
            if (typeAssets == null)
            {
                typeAssets = typeFields.Keys.ToDictionary(
                                 type => type,
                                 type => AssetDatabase.FindAssets($"t:{type.Name}")
                                                         .Select(AssetDatabase.GUIDToAssetPath)
                                                         .SelectMany(path =>
                                                         {
                                                             var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                                                             if (asset && type.IsAssignableFrom(asset.GetType()) && AssetDatabase.IsMainAsset(asset))
                                                                 return Array.Empty<ScriptableObject>().Append(asset);
                                                             else
                                                                 return AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
                                                                                     .Where(rep => type.IsAssignableFrom(rep.GetType()))
                                                                                     .OfType<ScriptableObject>();
                                                         })
                                                         .ToList());
            }
        }

        private static IEnumerable<FieldInfo> GetTypeFields(Type type)
        {
            return type.GetFields()
                                      .Where(field => field.GetCustomAttributes<PathReferenceResolverAttribute>().Any())
                                      .Where(field => field.FieldType == typeof(string) || field.FieldType == typeof(string[]) || field.FieldType == typeof(List<string>));
        }

        private static Regex referenceIdentifier = new Regex("\\%(.*?)\\%");
        public static string ResolvePath(string input, Pipeline pipeline, UnityEngine.Object caller)
        {
            var result = input;
            var pathReferenceGuids = AssetDatabase.FindAssets($"t:{nameof(PathReference)}");
            var pathReferencePaths = pathReferenceGuids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            var pathReferences = pathReferencePaths.Select(AssetDatabase.LoadAssetAtPath<PathReference>).ToArray();
            var pathReferenceDictionary = pathReferences.ToDictionary(pr => pr.name);

            Match match = referenceIdentifier.Match(result);
            while (match != null && !string.IsNullOrEmpty(match.Value))
            {
                var matchValue = match.Value.Trim('%');
                if (!pathReferenceDictionary.ContainsKey(matchValue))
                {
                    EditorGUIUtility.PingObject(caller);
                    throw new KeyNotFoundException($"No PathReference named \"{matchValue}\" found in AssetDatabase");
                }
                var replacement = pathReferenceDictionary[matchValue].GetPath(pipeline);
                result = result.Replace(match.Value, replacement);
                match = match.NextMatch();
            }

            return result;
        }


        public override Type ElementType => typeof(PathComponent);

        public override bool SupportsType(Type type) => ElementType.IsAssignableFrom(type);

        public string GetPath(Pipeline pipeline)
        {
            return Data.OfType<PathComponent>().Select(d => d.GetPath(this, pipeline)).Aggregate(Combine);
        }

        [SerializeField, HideInInspector]
        private string lastName;
        private bool UpdateReferences;

        void OnEnable()
        {
            if (!referenceLookup.ContainsKey(name))
            {
                referenceLookup[name] = this;
                InitializeLookup();
            }
            else
                Debug.LogError($@"Cannot add PathReference named {name} to ReferenceLookup, {name} already exists.

PathReference In ReferenceLookup: {AssetDatabase.GetAssetPath(referenceLookup[name])}

This PathReference: {AssetDatabase.GetAssetPath(this)}
");
        }
        public void OnBeforeSerialize()
        {
            if (lastName != name)
            {
                InitializeLookup();
                if (!referenceLookup.ContainsKey(name))
                {
                    var changed = new List<SerializedObject>();
                    foreach (var type in typeAssets.Keys)
                    {
                        if (typeAssets[type].Any())
                            foreach (var asset in typeAssets[type])
                            {
                                var soAsset = new SerializedObject(asset);
                                EditorGUI.BeginChangeCheck();
                                foreach (var fieldName in typeFields[type])
                                {
                                    var property = soAsset.FindProperty(fieldName);
                                    if (property == null) continue;
                                    if (property.arrayElementType == "string")
                                        for (int i = 0; i < property.arraySize; i++)
                                        {
                                            if (UpdatePathReferenceValue(property.GetArrayElementAtIndex(i)))
                                                changed.Add(soAsset);
                                        }
                                    else if (UpdatePathReferenceValue(property))
                                        changed.Add(soAsset);
                                }
                            }
                    }
                    if (changed.Any())
                    {
                        Debug.Log($"PathReference: Updated references of {lastName} to {name}");
                        foreach (var so in changed)
                        {
                            so.SetIsDifferentCacheDirty();
                            so.ApplyModifiedProperties();
                        }
                        AssetDatabase.SaveAssets();
                    }
                }

                if (referenceLookup.ContainsKey(lastName))
                    referenceLookup.Remove(lastName);

                lastName = name;
            }
        }

        private bool UpdatePathReferenceValue(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.String)
                return false;
            var value = property.stringValue;
            if (value.Contains($"%{lastName}%"))
            {
                value = value.Replace($"%{lastName}%", $"%{name}%");
                property.stringValue = value;
                return true;
            }
            return false;
        }

        public void OnAfterDeserialize()
        {

        }

        public override string ElementTemplate => $@"
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Manifests;

namespace {{0}}
{{{{
    public class {{1}} : PathComponent
    {{{{
        public override string GetPath({nameof(PathReference)} output, Manifest manifest, Pipeline pipeline)
        {{{{
            return base.GetPath(output, manifest, pipeline);
        }}}}
    }}}}
}}}}
";
    }
}