#if UNITY_EDITOR && UNITY_2017
using System.IO;
using System.Linq;
using UnityEditor;

namespace PassivePicasso.ThunderKit.PreConfig
{
	[InitializeOnLoad]
	public class PreConfigStep
	{
		static PreConfigStep()
		{
			var pluginsDir = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "plugins");
			var editorPath = Path.GetDirectoryName(EditorApplication.applicationPath);
			var path = Path.Combine(editorPath, "Data", "MonoBleedingEdge", "lib", "mono", "gac");
			var siocfs = "System.IO.Compression.FileSystem.dll";
			var sioc = "System.IO.Compression.dll";
			var destFileSystemPath = Path.Combine(pluginsDir, siocfs);
			var destCompressionPath = Path.Combine(pluginsDir, sioc);
			var refresh = false;

			if (!Directory.Exists(pluginsDir)) Directory.CreateDirectory(pluginsDir);

			if (!File.Exists(destCompressionPath))
			{
				var compressionDll = Directory.EnumerateFiles(path, sioc, SearchOption.AllDirectories).FirstOrDefault();
				File.Copy(compressionDll, destCompressionPath, true);
				refresh = true;
			}

			if (!File.Exists(destFileSystemPath))
			{
				var fileSystemDll = Directory.EnumerateFiles(path, siocfs, SearchOption.AllDirectories).FirstOrDefault();
				File.Copy(fileSystemDll, destFileSystemPath, true);
				refresh = true;
			}

			if(refresh) AssetDatabase.Refresh();
		}
	}
}
#endif