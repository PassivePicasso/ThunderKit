using System;
using System.IO;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using UnityEngine;

namespace ThunderKit.Core.Config.Common
{
    public class GetBitness : OptionalExecutor
    {
        public override int Priority => Constants.Priority.GetBitness;

        public override void Execute()
        {
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            if (Application.platform != RuntimePlatform.WindowsEditor)
                throw new ArgumentException("GetBitness isn't compatible with non-windows operating systems", "Application.platform");

            var assembly = Path.Combine(settings.GamePath, settings.GameExecutable);
            using (var stream = File.OpenRead(assembly))
            using (var binStream = new BinaryReader(stream))
            {
                stream.Seek(0x3C, SeekOrigin.Begin);
                if (binStream.PeekChar() != -1)
                {
                    var e_lfanew = binStream.ReadInt32();
                    stream.Seek(e_lfanew + 0x4, SeekOrigin.Begin);
                    var cpuType = binStream.ReadUInt16();
                    if (cpuType == 0x8664)
                    {
                        settings.Is64Bit = true;
                        return;
                    }
                }
            }
            settings.Is64Bit = false;
        }
    }
}