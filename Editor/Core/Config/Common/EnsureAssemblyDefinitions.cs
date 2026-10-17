using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderKit.Core.Config.Common
{
    public class EnsureAssemblyDefinitions : OptionalExecutor
    {
        public override int Priority => int.MaxValue - 150;

        public override string Description =>
            "Checks that all C# files in the Assets folder are covered by an Assembly Definition (.asmdef)" +
#if UNITY_2019_2_OR_NEWER
            ", or by an Assembly Definition Reference (.asmref) with a valid Assembly Definition reference value. " +
#else
            ". " +
#endif
            Environment.NewLine + 
            Environment.NewLine +
            "Orphaned scripts not under any .asmdef cause Unity to compile them into Assembly-CSharp, " +
            "which can conflict with game assemblies. Disable at your own risk.";

        public override bool Execute()
        {
            var asmdefDirs = RetrieveAsmdefDirs();

            var orphaned = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories)
                .Where(cs => !IsUnderAsmdef(Path.GetFullPath(cs), asmdefDirs))
                .ToList();

            if (orphaned.Count == 0)
                return true;

            const int maxListed = 10;
            var dialog = new StringBuilder();
            dialog.AppendLine($"{orphaned.Count} C# file(s) in Assets/ are not covered by an Assembly Definition (.asmdef).");
            dialog.AppendLine();
            for (int i = 0; i < Math.Min(orphaned.Count, maxListed); i++)
                dialog.AppendLine(orphaned[i]);
            if (orphaned.Count > maxListed)
                dialog.AppendLine($"... and {orphaned.Count - maxListed} more (see Console for full list)");
            dialog.AppendLine();
            dialog.AppendLine("You can do one of the following things:");
            dialog.AppendLine("* Create an .asmdef in the same folder or a parent folder of each file.");
#if UNITY_2019_2_OR_NEWER
            dialog.AppendLine("* Create an .asmref in the same folder or a parent folder of each file, ensuring that it's referencing a valid AssemblyDefinitionAsset.");
#endif
            dialog.AppendLine("* Move the files under an existing .asmdef");

            EditorUtility.DisplayDialog("Orphaned C# Scripts Detected", dialog.ToString(), "OK");

            var console = new StringBuilder();
            console.AppendLine($"Import aborted: {orphaned.Count} C# file(s) found in Assets/ that are not managed by any Assembly Definition (.asmdef).");
            console.AppendLine("Add or extend an .asmdef to cover these files before importing:");
            foreach (var path in orphaned)
                console.AppendLine($"  {path}");

            throw new Exception(console.ToString());
        }

        private HashSet<string> RetrieveAsmdefDirs()
        {
            IEnumerable<string> assemblyDefs = Directory.GetFiles("Assets", "*.asmdef", SearchOption.AllDirectories);

            //2019.2 introduces AssemblyReferences, consider these as well if we are in 2019.2 or newer
#if UNITY_2019_2_OR_NEWER
            assemblyDefs = assemblyDefs.Concat(GetValidAsmRefPaths());
#endif

            return new HashSet<string>(assemblyDefs.Select(f => Path.GetFullPath(Path.GetDirectoryName(f))), StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsUnderAsmdef(string csFullPath, HashSet<string> asmdefDirs)
        {
            var dir = Path.GetDirectoryName(csFullPath);
            while (dir != null)
            {
                if (asmdefDirs.Contains(dir)) return true;
                var parent = Path.GetDirectoryName(dir);
                if (parent == dir) break;
                dir = parent;
            }
            return false;
        }

#if UNITY_2019_2_OR_NEWER
        //C# representation of an AssemblyDefinitionReference (.asmref) json data
        [Serializable]
        struct AsmRef
        {
            //This is the only data stored in the json file, a "Reference" string. This string can either be the AssemblyDefinition name (IE: "MyAssembly"), or the GUID of the AssemblyDefinition (IE: "GUID:48ca85826e6646842b586677c033f60d").
            [SerializeField]
            private string reference;

            /// <summary>
            /// Returns the AssemblyDefinitionAsset GUID that this AssemblyDefinitionReference Asset contains
            /// </summary>
            public string GetAssemblyReferenceGUID()
            {
                if (string.IsNullOrWhiteSpace(reference))
                {
                    return "";
                }

                AssemblyDefinitionReferenceType referenceType = CompilationPipeline.GetAssemblyDefinitionReferenceType(reference);

                switch (referenceType)
                {
                    //If the reference is being stored by name, get the AssemblyDefinitionFilePath from it's name, then the GUID via the asset path
                    case AssemblyDefinitionReferenceType.Name:
                        {
                            string path = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(reference);
                            return AssetDatabase.AssetPathToGUID(path);
                        }
                    //If it's being stored by GUID, just retrieve it as is.
                    case AssemblyDefinitionReferenceType.Guid:
                        return CompilationPipeline.AssemblyDefinitionReferenceGUIDToGUID(reference);
                }

                return "";
            }
        }

        private IEnumerable<string> GetValidAsmRefPaths()
        {
            //Checks wether the AssemblyDefinitionReferenceAsset path is "Valid" (IE: it's "Assembly Definition" field is set to an AssemblyDefinitionAsset"
            bool IsValidAsmRefFile(string asmRefFilePath)
            {
                AssemblyDefinitionReferenceAsset assemblyDefinitionReferenceAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionReferenceAsset>(asmRefFilePath);
                AsmRef asmRef = JsonUtility.FromJson<AsmRef>(assemblyDefinitionReferenceAsset.text);

                string assemblyDefinitionGUID = asmRef.GetAssemblyReferenceGUID();
                string assemblyDefinitionPath = AssetDatabase.GUIDToAssetPath(assemblyDefinitionGUID);
                return AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assemblyDefinitionPath);
            }

            return Directory.GetFiles("Assets", "*.asmref", SearchOption.AllDirectories)
                .Where(IsValidAsmRefFile);
        }
#endif
    }
}
