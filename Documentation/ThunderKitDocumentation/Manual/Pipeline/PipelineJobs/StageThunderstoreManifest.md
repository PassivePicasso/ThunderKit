---
{ 
	"title" : "StageThunderstoreManifest",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Pipeline_2X_Icon" ]
}

---

[StageThunderstoreManifest](assetlink://GUID/dc52389347ae9634bbb7e74eba886518) creates Thunderstore manifest.json files

## Required ManifestDatums

* [ThunderstoreData](documentation://GUID/41a367ca4a0088f46b4327f0813dafdc)

## Remarks

The [StageThunderstoreManifest](assetlink://GUID/dc52389347ae9634bbb7e74eba886518) PipelineJob looks for ThunderstoreData ManifestDatums and will output a manifest.json to each StaginPath on each ThunderstoreData ManifestDatum attached to the current Manifests.
