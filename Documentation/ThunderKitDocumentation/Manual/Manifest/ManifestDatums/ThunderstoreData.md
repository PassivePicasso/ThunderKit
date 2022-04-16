---
{ 
	"title" : "ThunderstoreData",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Manifest_2X_Icon" ]
}

---

The [ThunderstoreData](assetlink://GUID/e0a82fec78ebc734d9ad1346cd40b5f9) ManifestDatum stores a URL and identifies the Manifest as being a Thunderstore compatible Manifest.

## Fields
* **Url**
  - Project Site url

## Inherited Fields

* **Staging Paths**
  - A list of destinations to deploy manifest.json to
  - Supports PathReferences

## PipelineJobs

* [StageThunderstoreManifest](documentation://GUID/74a0394c4eaea384e89e7a3688053c2b)
  - Generates a manifest.json file compatible with Thunderstore and outputs it to **StagingPaths** defined in this ManifestDatum.

## Remarks

Thunderstore Packages are required to contain a manifest.json file.

This ManifestDatum collects additional Thunderstore specific information and identifies the Manifest as being Thunderstore compatible.

The [StageThunderstoreManifest](assetlink://GUID/dc52389347ae9634bbb7e74eba886518) PipelineJob looks for this ManifestDatum and will output a Manifest.json to  each StaginPath on each ThunderstoreData ManifestDatum attached to the current Manifests.
