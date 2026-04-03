using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;

namespace ThunderKit.Core.Config.Common
{
    public class EnsureAssemblyDefinitions : OptionalExecutor
    {
        public override int Priority => int.MaxValue - 150;

        public override string Description =>
            "Checks that all C# files in the Assets folder are covered by an Assembly Definition (.asmdef). " +
            "Orphaned scripts not under any .asmdef cause Unity to compile them into Assembly-CSharp, " +
            "which can conflict with game assemblies. Disable at your own risk.";

        public override bool Execute()
        {
            var asmdefDirs = new HashSet<string>(
                Directory.GetFiles("Assets", "*.asmdef", SearchOption.AllDirectories)
                    .Select(f => Path.GetFullPath(Path.GetDirectoryName(f))),
                StringComparer.OrdinalIgnoreCase);

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
            dialog.Append("Create an .asmdef in the same folder or a parent folder of each file, or move the files under an existing .asmdef.");

            EditorUtility.DisplayDialog("Orphaned C# Scripts Detected", dialog.ToString(), "OK");

            var console = new StringBuilder();
            console.AppendLine($"Import aborted: {orphaned.Count} C# file(s) found in Assets/ that are not managed by any Assembly Definition (.asmdef).");
            console.AppendLine("Add or extend an .asmdef to cover these files before importing:");
            foreach (var path in orphaned)
                console.AppendLine($"  {path}");

            throw new Exception(console.ToString());
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
    }
}
