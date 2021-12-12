---
{ 
	"pageStylePath" : "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconUrl" : "Packages/com.passivepicasso.thunderkit/Documentation/graphics/TK_Pipeline_2X_Icon.png",
	"iconClasses" : [ "header-icon" ]
}

---

[StageManifestFiles](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Pipelines/Jobs/StageManifestFiles.cs) will copy Assets to StagingPaths defined in the Files ManifestDatum

## Required ManifestDatums

* [Files](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Manifests/Datum/Files.cs)

## Remarks

This pipeline will copy all assets from each Files ManifestDatum attached to Manifests being processed by the current pipeline.

This can be used to copy any files in that are assets under the Assets folder of hte project out to locations specified in the Files ManifestDatum's Staging Paths.