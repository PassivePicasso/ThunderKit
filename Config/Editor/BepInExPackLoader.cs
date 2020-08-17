#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Thunderstore.Editor;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Directory = System.IO.Directory;
using Path = System.IO.Path;

namespace PassivePicasso.ThunderKit.Config.Editor
{
    public class BepInExPackLoader
    {
        private const string ROS_Temp = "ros_temp";
        private const string Referenced = "Referenced";

        readonly static string TempDir = Path.Combine(Directory.GetCurrentDirectory(), ROS_Temp);
        readonly static string ReferencesPath = Path.Combine(TempDir, Referenced);
        readonly static string PluginsPath = Path.Combine("Assets", "plugins");
        readonly static string BepInExPath = Path.Combine(PluginsPath, "BepInEx.dll");
        readonly static string HarmonyPath = Path.Combine(PluginsPath, "0Harmony.dll");
        readonly static string RuntimeDetourPath = Path.Combine(PluginsPath, "MonoMod.RuntimeDetour.dll");
        readonly static string UtilsPath = Path.Combine(PluginsPath, "MonoMod.Utils.dll");
        readonly static string CecilPath = Path.Combine(PluginsPath, "Mono.Cecil.dll");

        public static async Task DownloadBepinex(int pageIndex = 1)
        {
            Debug.Log("Acquiring latest BepInExPack");

            var bepinexPacks = ThunderLoad.LookupPackage("BepInExPack");
            var bepinex = bepinexPacks.FirstOrDefault();

            if (bepinex == null)
            {
                Debug.LogError("BepInEx Package not found.");
                return;
            }

            Debug.Log("Found latest BepInExPack");

            var extractedDir = Path.Combine(ReferencesPath, bepinex.name);

            if (!Directory.Exists(PluginsPath))
            {
                Directory.CreateDirectory(PluginsPath);
                AssetDatabase.Refresh();
            }

            if (Directory.Exists(TempDir))
                Directory.Delete(TempDir, true);

            Directory.CreateDirectory(TempDir);
            Directory.CreateDirectory(ReferencesPath);

            Debug.Log("Downloading latest BepInExPack");

            string filePath = Path.Combine(TempDir, $"{bepinex.full_name}.zip");

            await ThunderLoad.DownloadPackageAsync(bepinex, filePath);

            using (var fileStream = File.OpenRead(filePath))
            using (var archive = new ZipArchive(fileStream))
            {
                archive.ExtractToDirectory(extractedDir);

                var bepinexDll = Directory.GetFiles(extractedDir, "BepInEx.dll", SearchOption.AllDirectories).First();
                var harmonyDll = Directory.GetFiles(extractedDir, "0Harmony.dll", SearchOption.AllDirectories).First();
                var RuntimeDetourDll = Directory.GetFiles(extractedDir, "MonoMod.RuntimeDetour.dll", SearchOption.AllDirectories).First();
                var UtilsDll = Directory.GetFiles(extractedDir, "MonoMod.Utils.dll", SearchOption.AllDirectories).First();
                var cecilDll = Directory.GetFiles(extractedDir, "Mono.Cecil.dll", SearchOption.AllDirectories).First();

                File.Copy(bepinexDll, BepInExPath, true);
                File.Copy(harmonyDll, HarmonyPath, true);
                File.Copy(RuntimeDetourDll, RuntimeDetourPath, true);
                File.Copy(UtilsDll, UtilsPath, true);
                File.Copy(cecilDll, CecilPath, true);

                File.WriteAllText($"{BepInExPath}.meta", ConfigureProject.MetaData);
                File.WriteAllText($"{HarmonyPath}.meta", ConfigureProject.MetaData);
                File.WriteAllText($"{RuntimeDetourPath}.meta", ConfigureProject.MetaData);
                File.WriteAllText($"{UtilsPath}.meta", ConfigureProject.MetaData);
                File.WriteAllText($"{CecilPath}.meta", ConfigureProject.MetaData);
            }

            Debug.Log("Cleaning up BepInExPack temporary files");
            foreach (var file in Directory.EnumerateFiles(TempDir))
            {
                try
                {
                    Debug.Log($"Deleting File: {file}");
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            Directory.Delete(TempDir, true);

            AssetDatabase.Refresh();
        }
    }
}
#endif