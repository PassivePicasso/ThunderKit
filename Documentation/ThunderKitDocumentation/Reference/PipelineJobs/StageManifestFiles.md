---
{ 
	"title" : "StageManifestFiles",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Pipeline_2X_Icon" ]
}

---

[StageManifestFiles](assetlink://GUID/3570c76eb7a5c3c45942d9295a150917) will copy Assets to StagingPaths defined in the Files ManifestDatum

## Required ManifestDatums

* [Files](assetlink://GUID/4b243ff405b33b94dbf5b6775dd9aa33)

## Remarks

This pipeline will copy all assets from each Files ManifestDatum attached to Manifests being processed by the current pipeline.

This can be used to copy any files in that are assets under the Assets folder of hte project out to locations specified in the Files ManifestDatum's Staging Paths.