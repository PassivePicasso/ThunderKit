using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AssetsTools.NET
{
    public class AssetTypeTemplateField
    {
        public string name;
        public string type;
        public EnumValueTypes valueType;
        public bool isArray;
        public bool align;
        public bool hasValue;
        public int childrenCount;
        public int version;
        public AssetTypeTemplateField[] children;
        public AssetTypeTemplateField parent;

        public bool From0D(Type_0D u5Type, int fieldIndex)
        {
            TypeField_0D field = u5Type.typeFieldsEx[fieldIndex];
            name = field.GetNameString(u5Type.stringTable);
            type = field.GetTypeString(u5Type.stringTable);
            valueType = AssetTypeValueField.GetValueTypeByTypeName(type);
            isArray = field.isArray == 1;
            align = (field.flags & 0x4000) != 0x00;
            hasValue = valueType != EnumValueTypes.None;
            version = field.version;

            List<int> childrenIndexes = new List<int>();
            int thisDepth = u5Type.typeFieldsEx[fieldIndex].depth;
            for (int i = fieldIndex + 1; i < u5Type.typeFieldsExCount; i++)
            {
                if (u5Type.typeFieldsEx[i].depth == thisDepth + 1)
                {
                    childrenCount++;
                    childrenIndexes.Add(i);
                }
                if (u5Type.typeFieldsEx[i].depth <= thisDepth) break;
            }
            children = childrenCount == 0 ? Net35Polyfill.ArrayEmpty<AssetTypeTemplateField>() : new AssetTypeTemplateField[childrenCount];
            int child = 0;
            for (int i = fieldIndex + 1; i < u5Type.typeFieldsExCount; i++)
            {
                if (u5Type.typeFieldsEx[i].depth == thisDepth + 1)
                {
                    children[child] = new AssetTypeTemplateField();
                    children[child].From0D(u5Type, childrenIndexes[child]);
                    children[child].parent = this;
                    child++;
                }
                if (u5Type.typeFieldsEx[i].depth <= thisDepth) break;
            }

            //Apparently, there can be a case where string child is not an array but just int (ExposedReferenceTable field in PlayableDirector class before 2018.4.29)
            //For now, just set it to whatever type this child is. Maybe think something better later
            if (valueType == EnumValueTypes.String && children[0].valueType == EnumValueTypes.Int32)
            {
                valueType = children[0].valueType;
                childrenCount = 0;
                children = Net35Polyfill.ArrayEmpty<AssetTypeTemplateField>();
            }

            if (isArray)
            {
                if (children[1].valueType == EnumValueTypes.UInt8)
                {
                    valueType = EnumValueTypes.ByteArray;
                }
                else
                {
                    valueType = EnumValueTypes.Array;
                }
                hasValue = true;
            }

            return true;
        }

        public bool FromClassDatabase(ClassDatabaseFile file, ClassDatabaseType type, uint fieldIndex)
        {
            ClassDatabaseTypeField field = type.fields[(int)fieldIndex];
            name = field.fieldName.GetString(file);
            this.type = field.typeName.GetString(file);
            valueType = AssetTypeValueField.GetValueTypeByTypeName(this.type);
            isArray = field.isArray == 1;
            align = (field.flags2 & 0x4000) != 0x00;
            hasValue = valueType != EnumValueTypes.None;
            version = field.version;

            List<int> childrenIndexes = new List<int>();
            int thisDepth = type.fields[(int)fieldIndex].depth;
            for (int i = (int)fieldIndex + 1; i < type.fields.Count; i++)
            {
                if (type.fields[i].depth == thisDepth + 1)
                {
                    childrenCount++;
                    childrenIndexes.Add(i);
                }
                if (type.fields[i].depth <= thisDepth) break;
            }
            children = childrenCount == 0 ? Net35Polyfill.ArrayEmpty<AssetTypeTemplateField>() : new AssetTypeTemplateField[childrenCount];
            int child = 0;
            for (int i = (int)fieldIndex + 1; i < type.fields.Count; i++)
            {
                if (type.fields[i].depth == thisDepth + 1)
                {
                    children[child] = new AssetTypeTemplateField();
                    children[child].FromClassDatabase(file, type, (uint)childrenIndexes[child]);
                    children[child].parent = this;
                    child++;
                }
                if (type.fields[i].depth <= thisDepth) break;
            }

            //Apparently, there can be a case where string child is not an array but just int (ExposedReferenceTable field in PlayableDirector class before 2018.4.29)
            //For now, just set it to whatever type this child is. Maybe think something better later
            if (valueType == EnumValueTypes.String && !children[0].isArray && children[0].valueType != EnumValueTypes.None)
            {
                valueType = children[0].valueType;
                childrenCount = 0;
                children = Net35Polyfill.ArrayEmpty<AssetTypeTemplateField>();
            }

            if (isArray)
            {
                if (children[1].valueType == EnumValueTypes.UInt8)
                {
                    valueType = EnumValueTypes.ByteArray;
                }
                else
                {
                    valueType = EnumValueTypes.Array;
                }
                hasValue = true;
            }

            return true;
        }

        public AssetTypeValueField MakeValue(AssetsFileReader reader)
        {
            AssetTypeValueField valueField = new AssetTypeValueField();
            valueField.templateField = this;
            valueField = ReadType(reader, valueField);
            return valueField;
        }

        public AssetTypeValueField ReadType(AssetsFileReader reader, AssetTypeValueField valueField)
        {
            if (valueField.templateField.isArray)
            {
                if (valueField.templateField.childrenCount == 2)
                {
                    EnumValueTypes sizeType = valueField.templateField.children[0].valueType;
                    if (sizeType == EnumValueTypes.Int32 ||
                        sizeType == EnumValueTypes.UInt32)
                    {
                        if (valueField.templateField.valueType == EnumValueTypes.ByteArray)
                        {
                            valueField.childrenCount = 0;
                            valueField.children = Net35Polyfill.ArrayEmpty<AssetTypeValueField>();
                            int size = reader.ReadInt32();
                            byte[] data = reader.ReadBytes(size);
                            if (valueField.templateField.align) reader.Align();
                            AssetTypeByteArray atba = new AssetTypeByteArray();
                            atba.size = (uint)size;
                            atba.data = data;
                            valueField.value = new AssetTypeValue(EnumValueTypes.ByteArray, atba);
                        }
                        else
                        {
                            valueField.childrenCount = reader.ReadInt32();
                            valueField.children = valueField.childrenCount == 0 ? Net35Polyfill.ArrayEmpty<AssetTypeValueField>() : new AssetTypeValueField[valueField.childrenCount];
                            for (int i = 0; i < valueField.childrenCount; i++)
                            {
                                valueField.children[i] = new AssetTypeValueField();
                                valueField.children[i].templateField = valueField.templateField.children[1];
                                valueField.children[i] = ReadType(reader, valueField.children[i]);
                            }
                            if (valueField.templateField.align) reader.Align();
                            AssetTypeArray ata = new AssetTypeArray();
                            ata.size = valueField.childrenCount;
                            valueField.value = new AssetTypeValue(EnumValueTypes.Array, ata);
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid array value type! Found an unexpected " + sizeType.ToString() + " type instead!");
                    }
                }
                else
                {
                    throw new Exception("Invalid array!");
                }
            }
            else
            {
                EnumValueTypes type = valueField.templateField.valueType;
                if (type != 0) valueField.value = new AssetTypeValue(type, null);
                if (type == EnumValueTypes.String)
                {
                    int length = reader.ReadInt32();
                    valueField.value.Set(reader.ReadBytes(length));
                    reader.Align();
                }
                else
                {
                    valueField.childrenCount = valueField.templateField.childrenCount;
                    if (valueField.childrenCount == 0)
                    {
                        valueField.children = Net35Polyfill.ArrayEmpty<AssetTypeValueField>();
                        switch (valueField.templateField.valueType)
                        {
                            case EnumValueTypes.Int8:
                                valueField.value.Set(reader.ReadSByte());
                                if (valueField.templateField.align) reader.Align();
                                break;
                            case EnumValueTypes.UInt8:
                            case EnumValueTypes.Bool:
                                valueField.value.Set(reader.ReadByte());
                                if (valueField.templateField.align) reader.Align();
                                break;
                            case EnumValueTypes.Int16:
                                valueField.value.Set(reader.ReadInt16());
                                if (valueField.templateField.align) reader.Align();
                                break;
                            case EnumValueTypes.UInt16:
                                valueField.value.Set(reader.ReadUInt16());
                                if (valueField.templateField.align) reader.Align();
                                break;
                            case EnumValueTypes.Int32:
                                valueField.value.Set(reader.ReadInt32());
                                break;
                            case EnumValueTypes.UInt32:
                                valueField.value.Set(reader.ReadUInt32());
                                break;
                            case EnumValueTypes.Int64:
                                valueField.value.Set(reader.ReadInt64());
                                break;
                            case EnumValueTypes.UInt64:
                                valueField.value.Set(reader.ReadUInt64());
                                break;
                            case EnumValueTypes.Float:
                                valueField.value.Set(reader.ReadSingle());
                                break;
                            case EnumValueTypes.Double:
                                valueField.value.Set(reader.ReadDouble());
                                break;
                        }
                    }
                    else
                    {
                        valueField.children = new AssetTypeValueField[valueField.childrenCount];
                        for (int i = 0; i < valueField.childrenCount; i++)
                        {
                            valueField.children[i] = new AssetTypeValueField();
                            valueField.children[i].templateField = valueField.templateField.children[i];
                            valueField.children[i] = ReadType(reader, valueField.children[i]);
                        }
                        if (valueField.templateField.align) reader.Align();
                    }
                }
            }
            return valueField;
        }
    }
}
