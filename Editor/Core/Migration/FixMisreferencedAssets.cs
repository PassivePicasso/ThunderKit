using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Migration
{
    public static class FixMisreferencedAssets
    {
        [MenuItem("Tools/ThunderKit/Migration/Switch DLL references to CS references")]
        public static void Execute()
        {
            var selection = Selection.assetGUIDs;
            try
            {
                AssetDatabase.StartAssetEditing();
                var scriptRefRegex = new Regex("  m_Script: \\{fileID: (\\d*?), guid: (\\w*?), type: \\d\\}");
                var monoScripts = AssetDatabase.FindAssets("t:MonoScript")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                    .ToArray();

                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(asm =>
                    {
                        try { return asm.GetTypes(); }
                        catch { return Array.Empty<Type>(); }
                    })
                    .Distinct().ToArray();

                var monoScriptTable = new Dictionary<long, string>();
                foreach (var monoScript in monoScripts)
                {
                    var type = monoScript.GetClass();
                    if (type == null) continue;
                    var path = AssetDatabase.GetAssetPath(monoScript);
                    var guid = AssetDatabase.AssetPathToGUID(path);
                    var fileId = FileIdUtil.Compute(type);
                    if (!monoScriptTable.ContainsKey(fileId))
                        monoScriptTable[fileId] = guid;
                }

                var paths = Selection.assetGUIDs.Select(guid => AssetDatabase.GUIDToAssetPath(guid));
                foreach (var file in selection.Select(AssetDatabase.GUIDToAssetPath).Select(path => (path, lines: File.ReadLines(path))))
                {
                    var lines = file.lines.ToArray();
                    var changes = false;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        var match = scriptRefRegex.Match(line);
                        if (!match.Success) continue;

                        long fileId = long.Parse($"{match.Groups[1]}");
                        if (monoScriptTable.ContainsKey(fileId))
                        {
                            lines[i] = scriptRefRegex.Replace(line, $"  m_Script: {{fileID: 11500000, guid: {monoScriptTable[fileId]}, type: 3}}");
                            changes = true;
                        }
                    }
                    if (changes)
                    {
                        Debug.Log($"Changed {file.path}");
                        File.WriteAllLines(file.path, lines);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
                Selection.objects = null;
                Selection.objects = selection.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadMainAssetAtPath).ToArray();
            }
        }
    }
}
