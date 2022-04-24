using System;


namespace ThunderKit.Core.Data
{
    [Flags]
    public enum IncludedSettings
    {
        AudioManager = 1,
        ClusterInputManager = 2,
        DynamicsManager = 4,
        EditorBuildSettings = 8,
        EditorSettings = 16,
        GraphicsSettings = 32,
        InputManager = 64,
        NavMeshAreas = 128,
        NetworkManager = 256,
        Physics2DSettings = 512,
        PresetManager = 1024,
        ProjectSettings = 2048,
        QualitySettings = 4096,
        TagManager = 8192,
        TimeManager = 16384,
        UnityConnectSettings = 32768,
        VFXManager = 65536,
        XRSettings = 131072
    }
}