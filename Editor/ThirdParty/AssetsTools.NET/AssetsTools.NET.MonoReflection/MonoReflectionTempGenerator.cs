using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AssetsTools.NET.MonoReflection
{
    public class MonoReflectionTempGenerator : IMonoBehaviourTemplateGenerator
    {
        private UnityVersion unityVersion;
        private bool anyFieldIsManagedReference;

        public void Dispose()
        {

        }

        public AssetTypeTemplateField GetTemplateField(AssetTypeTemplateField baseField, string assemblyName, string nameSpace, string className, UnityVersion unityVersion)
        {
            // newer games don't have .dll
            if (assemblyName.EndsWith(".dll"))
            {
                assemblyName = assemblyName.Substring(0, assemblyName.Length - 4);
            }

            List<AssetTypeTemplateField> newFields = Read(assemblyName, nameSpace, className, unityVersion);

            AssetTypeTemplateField newBaseField = baseField.Clone();
            newBaseField.Children.AddRange(newFields);

            return newBaseField;
        }

        public List<AssetTypeTemplateField> Read(string assemblyName, string nameSpace, string typeName, UnityVersion unityVersion)
        {
            Assembly asmDef = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            return Read(asmDef, nameSpace, typeName, unityVersion);
        }

        public List<AssetTypeTemplateField> Read(Assembly assembly, string nameSpace, string typeName, UnityVersion unityVersion)
        {
            this.unityVersion = unityVersion;
            anyFieldIsManagedReference = false;
            List<AssetTypeTemplateField> children = new List<AssetTypeTemplateField>();

            RecursiveTypeLoad(assembly, nameSpace, typeName, children, CommonMonoTemplateHelper.GetSerializationLimit(unityVersion));
            return children;
        }

        private void RecursiveTypeLoad(Assembly assembly, string nameSpace, string typeName, List<AssetTypeTemplateField> attf, int availableDepth)
        {
            Type type = assembly.GetType($"{nameSpace}{(string.IsNullOrEmpty(nameSpace) ? "" : ".")}{typeName}");

            RecursiveTypeLoad(type, attf, availableDepth, true);
        }

        private void RecursiveTypeLoad(TypeDefWithSelfRef type, List<AssetTypeTemplateField> attf, int availableDepth, bool isRecursiveCall = false)
        {
            if (!isRecursiveCall)
            {
                availableDepth--;
            }

            string baseName = type.typeDef.BaseType.FullName;
            if (baseName != "System.Object" &&
                baseName != "UnityEngine.Object" &&
                baseName != "UnityEngine.MonoBehaviour" &&
                baseName != "UnityEngine.ScriptableObject")
            {
                TypeDefWithSelfRef typeDef = type.typeDef.BaseType;
                typeDef.AssignTypeParams(type);
                RecursiveTypeLoad(typeDef, attf, availableDepth, true);
            }

            attf.AddRange(ReadTypes(type, availableDepth));
        }

        private List<AssetTypeTemplateField> ReadTypes(TypeDefWithSelfRef type, int availableDepth)
        {
            List<FieldInfo> acceptableFields = GetAcceptableFields(type, availableDepth);
            List<AssetTypeTemplateField> localChildren = new List<AssetTypeTemplateField>();
            for (int i = 0; i < acceptableFields.Count; i++)
            {
                AssetTypeTemplateField field = new AssetTypeTemplateField();
                FieldInfo fieldDef = acceptableFields[i];
                TypeDefWithSelfRef fieldTypeDef = type.SolidifyType(fieldDef.FieldType);

                bool isArrayOrList = false;
                bool isPrimitive = false;
                bool derivesFromUEObject = false;
                bool isManagedReference = false;

                if (fieldTypeDef.typeRef.IsArray)
                {
                    isArrayOrList = fieldTypeDef.typeRef.GetArrayRank() == 1;
                }
                else if (fieldTypeDef.typeDef.FullName == "System.Collections.Generic.List`1")
                {
                    fieldTypeDef = fieldTypeDef.typeParamToArg.First().Value;
                    isArrayOrList = true;
                }
                
                field.Name = fieldDef.Name;
                if (isPrimitive = fieldTypeDef.typeDef.IsEnum)
                {
                    var enumType = Enum.GetUnderlyingType(fieldTypeDef.typeDef).FullName;
                    field.Type = CommonMonoTemplateHelper.ConvertBaseToPrimitive(enumType);
                }
                else if (isPrimitive = fieldTypeDef.typeDef.IsPrimitive)
                {
                    field.Type = CommonMonoTemplateHelper.ConvertBaseToPrimitive(fieldTypeDef.typeDef.FullName);
                }
                else if (fieldTypeDef.typeDef.FullName == "System.String")
                {
                    field.Type = "string";
                }
                else if (derivesFromUEObject = DerivesFromUEObject(fieldTypeDef))
                {
                    field.Type = $"PPtr<${fieldTypeDef.typeDef.Name}>";
                }
                else if (isManagedReference = fieldDef.GetCustomAttributes(true).Any(a => a.GetType().Name == "SerializeReference"))
                {
                    anyFieldIsManagedReference = true;
                    field.Type = "managedReference";
                }
                else
                {
                    field.Type = fieldTypeDef.typeDef.Name;
                }

                if (isPrimitive)
                {
                    field.Children = new List<AssetTypeTemplateField>();
                }
                else if (fieldTypeDef.typeDef.FullName == "System.String")
                {
                    field.Children = CommonMonoTemplateHelper.String();
                }
                else if (CommonMonoTemplateHelper.IsSpecialUnityType(fieldTypeDef.typeDef.FullName))
                {
                    field.Children = SpecialUnity(fieldTypeDef, availableDepth);
                }
                else if (derivesFromUEObject)
                {
                    field.Children = CommonMonoTemplateHelper.PPtr(unityVersion);
                }
                else if (isManagedReference)
                {
                    field.Children = CommonMonoTemplateHelper.ManagedReference(unityVersion);
                }
                else if (fieldTypeDef.typeDef.IsSerializable)
                {
                    field.Children = Serialized(fieldTypeDef, availableDepth);
                }

                field.ValueType = AssetTypeValueField.GetValueTypeByTypeName(field.Type);
                field.IsAligned = CommonMonoTemplateHelper.TypeAligns(field.ValueType);
                field.HasValue = field.ValueType != AssetValueType.None;

                if (isArrayOrList)
                {
                    if (isPrimitive || derivesFromUEObject)
                    {
                        field = CommonMonoTemplateHelper.Vector(field);
                    }
                    else
                    {
                        field = CommonMonoTemplateHelper.VectorWithType(field);
                    }
                }
                localChildren.Add(field);
            }

            if (anyFieldIsManagedReference && DerivesFromUEObject(type))
            {
                localChildren.Add(CommonMonoTemplateHelper.ManagedReferencesRegistry("references", unityVersion));
            }

            return localChildren;
        }

        private List<FieldInfo> GetAcceptableFields(TypeDefWithSelfRef typeDef, int availableDepth)
        {
            List<FieldInfo> validFields = new List<FieldInfo>();
            foreach (FieldInfo f in typeDef.typeDef.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var customAttributesTypes = f.GetCustomAttributes(true).Select(a => a.GetType()).ToArray();
                if (f.IsPublic ||
                    customAttributesTypes.Any(a => a.FullName == "UnityEngine.SerializeField") ||
                    customAttributesTypes.Any(a => a.FullName == "UnityEngine.SerializeReference")) //field is public or has exception attribute
                {
                    if (!f.IsStatic &&
                        !f.IsNotSerialized &&
                        !f.IsInitOnly &&
                        !f.IsLiteral) //field is not public, has exception attribute, readonly, or const
                    {
                        TypeDefWithSelfRef ft = typeDef.SolidifyType(f.FieldType);

                        if (TryGetListOrArrayElement(ft.typeRef, out TypeDefWithSelfRef elemType))
                        {
                            //Array are not serialized at and past the serialization limit
                            if (availableDepth < 0)
                            {
                                continue;
                            }

                            //Unity can't serialize collection of collections, ignoring it
                            if (TryGetListOrArrayElement(elemType, out _))
                            {
                                continue;
                            }
                            ft = elemType;
                        }
                        //Unity doesn't serialize a field of the same type as declaring type
                        //unless it inherits from UnityEngine.Object
                        else if (typeDef.typeDef.FullName == ft.typeDef.FullName && !DerivesFromUEObject(typeDef))
                        {
                            continue;
                        }

                        Type ftd = ft.typeDef;
                        if (ftd != null && IsValidDef(customAttributesTypes, ftd, availableDepth))
                        {
                            validFields.Add(f);
                        }
                    }
                }
            }
            return validFields;

        }

        private bool TryGetListOrArrayElement(TypeDefWithSelfRef fieldType, out TypeDefWithSelfRef elemType)
        {
            if (fieldType.typeRef.IsArray)
            {
                elemType = fieldType.typeRef.GetElementType();
                return true;
            }
            else if (fieldType.typeRef.IsGenericType && fieldType.typeDef.FullName == "System.Collections.Generic.List`1")
            {
                elemType = fieldType.typeParamToArg["T"];
                return true;
            }

            elemType = default;
            return false;
        }

        private bool IsValidDef(Type[] customAttributesTypes, Type typeDef, int availableDepth)
        {
            //Before 2020.1.0 you couldn't have fields of a generic type, so they should be ingored
            //https://unity.com/releases/editor/whats-new/2020.1.0
            if (typeDef.ContainsGenericParameters && unityVersion.major < 2020)
            {
                return false;
            }

            if (typeDef.IsPrimitive ||
                typeDef.FullName == "System.String")
            {
                return true;
            }

            //Unity doesn't support long enums
            if (typeDef.IsEnum)
            {
                var enumType = Enum.GetUnderlyingType(typeDef).FullName;
                return enumType != "System.Int64" && enumType != "System.UInt64";
            }

            //Value types are not affected by the serialization limit
            if (availableDepth < 0)
            {
                return typeDef.IsValueType && (typeDef.IsSerializable || CommonMonoTemplateHelper.IsSpecialUnityType(typeDef.FullName));
            }

            if (DerivesFromUEObject(typeDef) ||
                CommonMonoTemplateHelper.IsSpecialUnityType(typeDef.FullName))
            {
                return true;
            }

            if (customAttributesTypes.Any(a => a.Name == "SerializeReference"))
            {
                if (unityVersion.major == 2019 && unityVersion.minor == 3 && unityVersion.patch < 8 && typeDef.FullName == "System.Object")
                {
                    return false;
                }

                return !typeDef.IsValueType && !typeDef.ContainsGenericParameters;
            }

            if (CommonMonoTemplateHelper.IsAssemblyBlacklisted(typeDef.Assembly.GetName().Name, unityVersion))
            {
                return false;
            }

            return !typeDef.IsAbstract && typeDef.IsSerializable;
        }

        private bool DerivesFromUEObject(TypeDefWithSelfRef typeDef)
        {
            if (typeDef.typeDef.BaseType == null)
                return false;
            if (typeDef.typeDef.IsInterface)
                return false;
            if (typeDef.typeDef.BaseType.FullName == "UnityEngine.Object" ||
                typeDef.typeDef.FullName == "UnityEngine.Object")
                return true;
            if (typeDef.typeDef.BaseType.FullName != "System.Object")
                return DerivesFromUEObject(typeDef.typeDef.BaseType);
            return false;
        }

        private List<AssetTypeTemplateField> Serialized(TypeDefWithSelfRef type, int availableDepth)
        {
            List<AssetTypeTemplateField> types = new List<AssetTypeTemplateField>();
            RecursiveTypeLoad(type, types, availableDepth);
            return types;
        }

        private List<AssetTypeTemplateField> SpecialUnity(TypeDefWithSelfRef type, int availableDepth)
        {
            switch (type.typeDef.Name)
            {
                case "Gradient": return CommonMonoTemplateHelper.Gradient(unityVersion);
                case "AnimationCurve": return CommonMonoTemplateHelper.AnimationCurve(unityVersion);
                case "LayerMask": return CommonMonoTemplateHelper.BitField();
                case "Bounds": return CommonMonoTemplateHelper.AABB();
                case "BoundsInt": return CommonMonoTemplateHelper.BoundsInt();
                case "Rect": return CommonMonoTemplateHelper.Rectf();
                case "RectOffset": return CommonMonoTemplateHelper.RectOffset();
                case "Color32": return CommonMonoTemplateHelper.RGBAi();
                case "GUIStyle": return CommonMonoTemplateHelper.GUIStyle(unityVersion);
                case "Vector2Int": return CommonMonoTemplateHelper.Vector2Int();
                case "Vector3Int": return CommonMonoTemplateHelper.Vector3Int();
                default: return Serialized(type, availableDepth);
            };
        }
    }
}
