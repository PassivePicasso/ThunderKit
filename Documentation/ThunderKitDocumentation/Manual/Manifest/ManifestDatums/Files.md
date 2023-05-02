---
{ 
	"title" : "Files",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Manifest_2X_Icon" ]
}

---

The [Files](assetlink://GUID/4b243ff405b33b94dbf5b6775dd9aa33) ManifestDatum stores references to assets inside your unity project.

## Fields
* **Include Meta Files**
  - Include .meta files for the specified assets
* **Files**
  - An array of assets

## Inherited Fields

* **Staging Paths**
  - A list of destinations to deploy files to
  - Supports PathReferences

## PipelineJobs

* [StageManifestFiles](documentation://GUID/0e4c94f5c80d49545b1a92238b82f66a) 
  - Copies each asset referenced in each Files ManifestDatum to the output paths defined in its Staging Paths.

## Remarks

Use this ManifestDatum to specify and group files to be deployed by Pipelines using the [StageManifestFiles](assetlink://GUID/3570c76eb7a5c3c45942d9295a150917) PipelineJob