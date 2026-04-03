---
{ 
	"title" : "Import Project Settings",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/3b40885578be10f4785f1fa347e9fefa){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[ImportProjectSettings](documentation://GUID/f6ef601f07def774daf73785ec0540ea)

Imports Unity ProjectSettings from the game's `globalgamemanagers` binary file.
Uses AssetsTools.NET to parse the binary asset format and export settings as
YAML files that Unity can read. The exported files are copied into the project's
`ProjectSettings/` folder.

This extension executes at `2,000,000` priority (`Constants.Priority.ProjectSettingsImport`).

## Included Settings

The **IncludedSettings** field controls which settings are imported. This is a
flags field displayed on the Import Configuration settings page. If no flags are
selected the extension does nothing and returns immediately.

Available flags:

| Flag | Exported File |
|------|--------------|
| AudioManager | AudioManager.asset |
| ClusterInputManager | ClusterInputManager.asset |
| DynamicsManager | DynamicsManager.asset |
| EditorBuildSettings | EditorBuildSettings.asset |
| EditorSettings | EditorSettings.asset |
| GraphicsSettings | GraphicsSettings.asset |
| InputManager | InputManager.asset |
| NavMeshAreas | NavMeshAreas.asset |
| NetworkManager | NetworkManager.asset |
| Physics2DSettings | Physics2DSettings.asset |
| PresetManager | PresetManager.asset |
| ProjectSettings | ProjectSettings.asset |
| QualitySettings | QualitySettings.asset |
| TagManager | TagManager.asset |
| TimeManager | TimeManager.asset |
| UnityConnectSettings | UnityConnectSettings.asset |
| VFXManager | VFXManager.asset |
| XRSettings | XRSettings.asset |

## Asset Name Remapping

Some asset types in `globalgamemanagers` use internal names that differ from the
ProjectSettings filename Unity expects:

- **PhysicsManager** exports as `DynamicsManager.asset`
- **NavMeshProjectSettings** exports as `NavMeshAreas.asset`
- **PlayerSettings** exports as `ProjectSettings.asset`

All other types use their class name directly as the filename.

## Ignored Types

The following system types are skipped during export because they are not
relevant to project settings: PreloadData, AssetBundle, BuildSettings,
DelayedCallManager, MonoManager, ResourceManager,
RuntimeInitializeOnLoadManager, ScriptMapper, StreamingManager, and MonoScript.
