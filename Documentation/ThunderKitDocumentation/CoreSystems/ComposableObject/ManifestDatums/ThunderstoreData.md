---
{ 
	"pageStylePath" : "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Manifest_2X_Icon" ]
}

---

The [ThunderstoreData](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Integrations/Thunderstore/ThunderstoreData.cs) ManifestDatum stores a URL and identifies the Manifest as being a Thunderstore compatible Manifest.

## Fields
* **Url**
  - Project Site url

## Inherited Fields

* **Staging Paths**
  - A list of destinations to deploy manifest.json to
  - Supports PathReferences

## PipelineJobs

* [StageThunderstoreManifest](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Integrations/Thunderstore/StageThunderstoreManifest.cs)
  - Generates a manifest.json file compatible with Thunderstore and outputs it to **StagingPaths** defined in this ManifestDatum.

## Remarks

Thunderstore Packages are required to contain a manifest.json file.

This ManifestDatum collects additional Thunderstore specific information and identifies the Manifest as being Thunderstore compatible.

The [StageThunderstoreManifest](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Integrations/Thunderstore/StageThunderstoreManifest.cs) PipelineJob looks for this ManifestDatum and will output a Manifest.json to  each StaginPath on each ThunderstoreData ManifestDatum attached to the current Manifests.
