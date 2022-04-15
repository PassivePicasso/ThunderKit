---
{ 
	"title" : "StageDependencies",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Pipeline_2X_Icon" ]
}

---

[StageDependencies](assetlink://GUID/1b4f4581088b47744a114c282abc085d) deploys Dependencies found in Manifests that are not in the project Assets folder

## Fields
* **Staging Path**
  - Enable this to copy debugging information to StagingPaths 
  - Supports PathReferences

## Required ManifestDatums

* [ManifestIdentity](documentation://GUID/a94fe0e2c9006104bb1735bd177af5d7)

## Remarks

Stage Assemblies is how you deploy Assemblies defined in AssemblyDefinitions ManifestDatums.

StageAssemblies will execute for each Manifest in the Manifest hierarchy.
