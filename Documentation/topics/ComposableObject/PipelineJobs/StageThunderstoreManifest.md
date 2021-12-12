---
{ 
	"pageStylePath" : "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconUrl" : "Packages/com.passivepicasso.thunderkit/Documentation/graphics/TK_Pipeline_2X_Icon.png",
	"iconClasses" : [ "header-icon" ]
}

---

[StageThunderstoreManifest](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Integrations/Thunderstore/StageThunderstoreManifest.cs) creates Thunderstore manifest.json files

## Required ManifestDatums

* [ThunderstoreData](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Integrations/Thunderstore/ThunderstoreData.cs)

## Remarks

The [StageThunderstoreManifest](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Integrations/Thunderstore/StageThunderstoreManifest.cs) PipelineJob looks for ThunderstoreData ManifestDatums and will output a manifest.json to each StaginPath on each ThunderstoreData ManifestDatum attached to the current Manifests.
