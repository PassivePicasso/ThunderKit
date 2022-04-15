using AssetsExporter;
using System;
using System.IO;
using System.Threading.Tasks;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
#if UNITY_2019
using UnityEngine.UIElements;
#elif UNITY_2018
using UnityEngine.Experimental.UIElements;
#endif


namespace ThunderKit.Core.Config.Common
{
    [Serializable]
    public class ImportProjectSettings : Executor
    {
        private const string TemplatePath = Constants.SettingsTemplatesPath + "/ImportProjectSettings.uxml";
        public override int Priority => Constants.ConfigPriority.ProjectSettingsImport;

        public int IncludedSettings;

        public override async Task Execute()
        {
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            var classDataPath = Path.GetFullPath(Path.Combine(Constants.ThunderKitRoot, "Editor", "ThirdParty", "AssetsTools.NET", "classdata.tpk"));

            var unityVersion = Application.unityVersion;
            var gameManagerTemp = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "ImportedProjectSettings");
            var editorDirectory = Path.GetDirectoryName(EditorApplication.applicationPath);
            var executablePath = Path.Combine(settings.GamePath, settings.GameExecutable);
            GameExporter.ExportGlobalGameManagers(executablePath, gameManagerTemp, settings.GameDataPath, editorDirectory, classDataPath, unityVersion);

            var includedSettings = (IncludedSettings)IncludedSettings;
            foreach (IncludedSettings include in (IncludedSettings[])Enum.GetValues(typeof(IncludedSettings)))
            {
                if (!includedSettings.HasFlag(include)) continue;

                string settingName = $"{include}.asset";
                string settingPath = Path.Combine("ProjectSettings", settingName);
                string tempSettingPath = Path.Combine(gameManagerTemp, "ProjectSettings", settingName);
                if (!File.Exists(tempSettingPath)) continue;

                File.Copy(tempSettingPath, settingPath, true);
                //Update times as necessary to trigger unity import
                File.SetLastWriteTime(settingPath, DateTime.Now);
                File.SetLastAccessTime(settingPath, DateTime.Now);
                File.SetCreationTime(settingPath, DateTime.Now);
                AssetDatabase.ImportAsset(settingPath);
            }

            while (EditorApplication.isUpdating)
                await Task.Yield();
        }

        public override VisualElement CreateUI()
        {
            var importProjectSettingsUI = TemplateHelpers.LoadTemplateInstance(TemplatePath);
            importProjectSettingsUI.AddEnvironmentAwareSheets(Constants.ThunderKitSettingsTemplatePath);

            return importProjectSettingsUI;
        }
    }
}