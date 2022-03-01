using AssetRipper.VersionUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Extra.CldbTypeTreeConverters
{
    public static class TypeTreeDumperToCldb
    {
        public static ClassDatabaseFile ParseDump(string structsPath, string stringsPath)
        {
            if (!string.IsNullOrEmpty(stringsPath) && !File.Exists(stringsPath))
            {
                throw new ArgumentException($"Couldn't find \"{Path.GetFileName(stringsPath)}\" file");
            }
            if (!File.Exists(structsPath))
            {
                throw new ArgumentException($"Couldn't find \"{Path.GetFileName(structsPath)}\" file");
            }
            var commonStrings = string.IsNullOrEmpty(stringsPath) ? "" : Encoding.UTF8.GetString(File.ReadAllBytes(stringsPath));
            using (var file = File.OpenRead(structsPath))
            using (var reader = new AssetsFileReader(file))
            {
                reader.bigEndian = false;
                var unityVersion = reader.ReadNullTerminated();
                var platform = reader.ReadInt32();
                var hasTypeTree = reader.ReadByte();
                var typeCount = reader.ReadInt32();

                ReadTypes(typeCount, reader, out var types, out var localStrings);

                return new ClassDatabaseFile
                {
                    classes = types,
                    stringTable = Encoding.UTF8.GetBytes(NormalizeStringsInTypes(types, localStrings, commonStrings)),
                    header = new ClassDatabaseFileHeader
                    {
                        unityVersionCount = 1,
                        unityVersions = new[] { UnityVersion.Parse(unityVersion) },
                        header = "cldb",
                        fileVersion = 4,
                        flags = 0,
                    },
                    valid = true,
                    bodyParsed = true,
                };
            }
        }

        private static void ReadTypes(int typeCount, AssetsFileReader reader, out List<ClassDatabaseType> types, out List<string> localStrings)
        {
            types = new List<ClassDatabaseType>();
            localStrings = new List<string>();
            for (var i = 0; i < typeCount; i++)
            {
                var persistentTypeID = reader.ReadInt32();
                for (int j = 0, n = persistentTypeID < 0 ? 0x20 : 0x10; j < n; ++j)
                {
                    _ = reader.ReadByte();
                }
                var fieldsCount = reader.ReadInt32();
                var stringBufferCount = reader.ReadInt32();
                ReadFields(fieldsCount, reader, out var fields);
                types.Add(new ClassDatabaseType
                {
                    classId = persistentTypeID,
                    fields = fields,
                });
                var stringBuffer = new byte[stringBufferCount];
                for (var j = 0; j < stringBufferCount; j++)
                {
                    stringBuffer[j] = reader.ReadByte();
                }
                localStrings.Add(Encoding.UTF8.GetString(stringBuffer));
            }
        }

        private static void ReadFields(int fieldsCount, AssetsFileReader reader, out List<ClassDatabaseTypeField> fields)
        {
            fields = new List<ClassDatabaseTypeField>();
            for (var j = 0; j < fieldsCount; j++)
            {
                var version = reader.ReadUInt16();
                var level = reader.ReadByte();
                var typeFlags = reader.ReadByte();
                var typeStrOffset = reader.ReadUInt32();
                var nameStrOffset = reader.ReadUInt32();
                var byteSize = reader.ReadInt32();
                var index = reader.ReadUInt32();
                var metaFlags = reader.ReadUInt32();

                fields.Add(new ClassDatabaseTypeField
                {
                    version = version,
                    depth = level,
                    isArray = typeFlags,
                    typeName = new ClassDatabaseFileString
                    {
                        fromStringTable = true,
                        str = new ClassDatabaseFileString.TableString
                        {
                            stringTableOffset = typeStrOffset
                        }
                    },
                    fieldName = new ClassDatabaseFileString
                    {
                        fromStringTable = true,
                        str = new ClassDatabaseFileString.TableString
                        {
                            stringTableOffset = nameStrOffset
                        }
                    },
                    size = byteSize,
                    flags2 = metaFlags,
                });
            }
        }

        private static string NormalizeStringsInTypes(List<ClassDatabaseType> types, List<string> localStrings, string commonStrings)
        {
            var builder = new StringBuilder();
            var foundStrings = new Dictionary<string, int>();
            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                type.name = GetString(type.fields[0].typeName, i);
                for (var j = 0; j < type.fields.Count; j++)
                {
                    var newField = type.fields[j];
                    newField.fieldName = GetString(newField.fieldName, i);
                    newField.typeName = GetString(newField.typeName, i);
                    type.fields[j] = newField;
                }
            }
            return builder.ToString();

            ClassDatabaseFileString GetString(ClassDatabaseFileString fileString, int typeIndex)
            {
                var offset = fileString.str.stringTableOffset;
                string value;
                if (offset >= 0x80000000)
                {
                    offset -= 0x80000000;
                    value = NullTerminatedSubstring(commonStrings, (int)offset);
                }
                else
                {
                    value = NullTerminatedSubstring(localStrings[typeIndex], (int)offset);
                }
                if (!foundStrings.TryGetValue(value, out var valueOffset))
                {
                    foundStrings[value] = valueOffset = builder.Length;
                    builder.Append(value).Append('\0');
                }
                fileString.str.stringTableOffset = (uint)valueOffset;
                return fileString;
            }
        }

        private static string NullTerminatedSubstring(string str, int start)
        {
            return str.Substring(start, str.IndexOf('\0', start) - start);
        }
    }
}
