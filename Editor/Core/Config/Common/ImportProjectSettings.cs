using AssetsExporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEngine;
using System.Collections;
#if UNITY_2019
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#elif UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif


namespace ThunderKit.Core.Config.Common
{
    [Serializable]
    public class ImportProjectSettings : OptionalExecutor
    {
        private const string TemplatePath = Constants.SettingsTemplatesPath + "/ImportProjectSettings.uxml";
        public override int Priority => Constants.ConfigPriority.ProjectSettingsImport;

        public int IncludedSettings;

        public override void Execute()
        {
            if (IncludedSettings == 0) return;

            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            var classDataPath = Path.GetFullPath(Path.Combine(Constants.ThunderKitRoot, "Editor", "ThirdParty", "AssetsTools.NET", "classdata.tpk"));

            var unityVersion = Application.unityVersion;
            var gameManagerTemp = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "ImportedProjectSettings");
            var editorDirectory = Path.GetDirectoryName(EditorApplication.applicationPath);
            var executablePath = Path.Combine(settings.GamePath, settings.GameExecutable);
            GameExporter.ExportGlobalGameManagers(executablePath, gameManagerTemp, settings.GameDataPath, editorDirectory, classDataPath, unityVersion);

            var includedSettings = (IncludedSettings)IncludedSettings;
            var importedSettings = new List<string>();
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
                importedSettings.Add(settingPath);
            }

            AssetDatabase.ImportAsset(importedSettings[0]);

            var escape = false;
            while (EditorApplication.isUpdating && !escape)
            {
                var x = escape;
            }
        }

        protected override VisualElement CreateProperties()
        {
            var importProjectSettingsUI = TemplateHelpers.LoadTemplateInstance(TemplatePath);
            importProjectSettingsUI.AddEnvironmentAwareSheets(Constants.ThunderKitSettingsTemplatePath);

            return importProjectSettingsUI;
        }
    }
}