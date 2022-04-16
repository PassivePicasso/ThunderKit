---
{ 
	"title" : "StageAssemblies",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Pipeline_2X_Icon" ]
}

---

[StageAssemblies](assetlink://GUID/b5b20fac9c71fd64183cb7a8f359d73a) copies Assemblies built by unity out to StagingPaths

## Fields
* **Stage Debug Database**
  - Enable this to copy debugging information to StagingPaths 
  - Additional steps are required to get debug information displaying line numbers
* **Prefer Player Builds**
  - Player Builds are release builds of assemblies which include optimization for release and removes debugging information
  - These should generally be used for final release testing and performance testing
  - Updated Player Builds are only available after an AssetBundle build has occurred

## Required ManifestDatums

* [AssemblyDefinitions](documentation://GUID/cef5acb6a795c5d4d9031261ea82e891)

## Remarks

Stage Assemblies is how you deploy Assemblies defined in AssemblyDefinitions ManifestDatums.

StageAssemblies will execute for each Manifest in the Manifest hierarchy.
