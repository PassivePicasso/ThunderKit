using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

            var sb = new StringBuilder();
            sb.AppendLine($"Import aborted: {orphaned.Count} C# file(s) found in Assets/ that are not managed by any Assembly Definition (.asmdef).");
            sb.AppendLine("Add or extend an .asmdef to cover these files before importing:");
            foreach (var path in orphaned)
                sb.AppendLine($"  {path}");

            throw new Exception(sb.ToString());
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
