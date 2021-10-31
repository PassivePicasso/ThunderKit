using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static ThunderKit.Core.Pipelines.LogEntry;

namespace ThunderKit.Core.Pipelines
{
    public class PipelineLog : ScriptableObject
    {
        public Pipeline pipeline;

        [SerializeField, HideInInspector]
        private SDateTime creationDate;
        public DateTime CreatedDate => creationDate;

        [SerializeField, HideInInspector]
        private List<LogEntry> entries = new List<LogEntry>();
        public IReadOnlyList<LogEntry> Entries => entries?.AsReadOnly();

        public void Log(LogEntry entry)
        {
            entries.Insert(0, entry);
            EditorUtility.SetDirty(this);
        }

        public static PipelineLog CreateLog(Pipeline pipeline)
        {
            var log = CreateInstance<PipelineLog>();
            EnsureLogDirectory(pipeline);
            var logPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/ThunderKitSettings/Logs/{pipeline.name}/Log 1.asset");
            var fileName = Path.GetFileNameWithoutExtension(logPath);
            log.name = fileName;
            log.pipeline = pipeline;
            log.creationDate = new SDateTime(DateTime.Now.Ticks);
            log.entries = new List<LogEntry>();
            var content = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
            ScriptableHelper.AssignIcon(log, content.image as Texture2D);

            AssetDatabase.CreateAsset(log, logPath);
            return log;
        }

        static void EnsureLogDirectory(Pipeline pipeline)
        {
            if (!AssetDatabase.IsValidFolder("Assets/ThunderKitSettings"))
                AssetDatabase.CreateFolder("Assets", "ThunderKitSettings");

            if (!AssetDatabase.IsValidFolder("Assets/ThunderKitSettings/Logs"))
                AssetDatabase.CreateFolder("Assets/ThunderKitSettings", "Logs");

            if (!AssetDatabase.IsValidFolder($"Assets/ThunderKitSettings/Logs/{pipeline.name}"))
                AssetDatabase.CreateFolder("Assets/ThunderKitSettings/Logs", pipeline.name);
        }
    }
}