using RainOfStages.RainOfStagesShared.AutoConfig.Editor.Thunderstore;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using Path = System.IO.Path;
using Directory = System.IO.Directory;
using System.Threading;
using System.IO.Compression;
using RainOfStages.AutoConfig;
using UnityEditor;

namespace RainOfStages.RainOfStagesShared.AutoConfig.Editor
{
    public class BepInExPackLoader
    {
        const string ThunderstoreIO = "https://thunderstore.io";
        const string PackageListApi = ThunderstoreIO + "/api/v2/package";
        const string PackageApi = ThunderstoreIO + "/package/download";
        private const string ROS_Temp = "ros_temp";
        private const string Referenced = "Referenced";

        public static async Task DownloadBepinex(int pageIndex = 1)
        {
            Debug.Log("Acquiring latest BepInExPack");
            var client = new WebClient();
            Uri address = new Uri($"{PackageListApi}/?page={pageIndex}");

            var response = await client.DownloadStringTaskAsync(address);

            var page = JsonUtility.FromJson<Page>(response);
            if (page == null || page.count == 0)
            {
                Debug.Log("No Thunderstore results found");
                return;
            }

            var bepinex = page.results.FirstOrDefault(package => package.name.Contains("BepInExPack"));

            if (bepinex == null)
            {
                _ = DownloadBepinex(pageIndex + 1);
                return;
            }
            Debug.Log("Found latest BepInExPack");

            var latestBepinex = bepinex.latest;

            var url = $"{PackageApi}/{bepinex.owner}/{bepinex.name}/{latestBepinex.version_number}/";

            var tempDir = Path.Combine(Directory.GetCurrentDirectory(), ROS_Temp);
            var referencesPath = Path.Combine(tempDir, Referenced);
            var extractedDir = Path.Combine(referencesPath, bepinex.name);
            var pluginsPath = Path.Combine("Assets", "plugins");

            var bepinexPath = Path.Combine(pluginsPath, "BepInEx.dll");
            var runtimeDetourPath = Path.Combine(pluginsPath, "MonoMod.RuntimeDetour.dll");
            var utilsPath = Path.Combine(pluginsPath, "MonoMod.Utils.dll");
            var cecilPath = Path.Combine(pluginsPath, "Mono.Cecil.dll");

            if (!Directory.Exists(pluginsPath))
            {
                Directory.CreateDirectory(pluginsPath);
                AssetDatabase.Refresh();
            }

            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(referencesPath);

            Debug.Log("Downloading latest BepInExPack");
            string filePath = Path.Combine(tempDir, $"{bepinex.full_name}.zip");
            await client.DownloadFileTaskAsync(url, filePath);

            using (var fileStream = File.OpenRead(filePath))
            using (var archive = new ZipArchive(fileStream))
            {
                archive.ExtractToDirectory(extractedDir);

                var bepinexDll = Directory.GetFiles(extractedDir, "BepInEx.dll", SearchOption.AllDirectories).First();
                var RuntimeDetourDll = Directory.GetFiles(extractedDir, "MonoMod.RuntimeDetour.dll", SearchOption.AllDirectories).First();
                var UtilsDll = Directory.GetFiles(extractedDir, "MonoMod.Utils.dll", SearchOption.AllDirectories).First();
                var cecilDll = Directory.GetFiles(extractedDir, "Mono.Cecil.dll", SearchOption.AllDirectories).First();

                if (!File.Exists(bepinexPath)) File.Copy(bepinexDll, bepinexPath);
                if (!File.Exists(runtimeDetourPath)) File.Copy(RuntimeDetourDll, runtimeDetourPath);
                if (!File.Exists(utilsPath)) File.Copy(UtilsDll, utilsPath);
                if (!File.Exists(cecilPath)) File.Copy(cecilDll, cecilPath);

                File.WriteAllText($"{bepinexPath}.meta", ConfigureProject.MetaData);
                File.WriteAllText($"{runtimeDetourPath}.meta", ConfigureProject.MetaData);
                File.WriteAllText($"{utilsPath}.meta", ConfigureProject.MetaData);
                File.WriteAllText($"{cecilPath}.meta", ConfigureProject.MetaData);
            }

            Debug.Log("Cleaning up BepInExPack temporary files");
            foreach (var file in Directory.EnumerateFiles(tempDir))
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

            Directory.Delete(tempDir, true);
            client.Dispose();
            AssetDatabase.Refresh();
        }
    }
}
