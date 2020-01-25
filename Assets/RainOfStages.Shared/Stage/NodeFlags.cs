// Decompiled with JetBrains decompiler
// Type: RainOfStages.NodeFlags
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5CDEE1C8-AFDF-42E2-A8DF-7BD1AE8DC681
// Assembly location: F:\Projects\RoR2_Modding\Risk of Rain 2\Risk of Rain 2_Data\Managed\Assembly-CSharp.dll

using System;

namespace RainOfStages
{
  [Flags]
  public enum NodeFlags : byte
  {
    None = 0,
    NoCeiling = 1,
    TeleporterOK = 2,
    NoCharacterSpawn = 4,
    NoChestSpawn = 8,
    NoShrineSpawn = 16, // 0x10
  }
}
