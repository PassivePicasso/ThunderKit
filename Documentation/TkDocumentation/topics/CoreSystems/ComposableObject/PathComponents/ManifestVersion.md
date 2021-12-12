---
{ 
	"pageStylePath" : "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconUrl" : "Packages/com.passivepicasso.thunderkit/Documentation/graphics/TK_PathReference_2X_Icon.png",
	"iconClasses" : [ "header-icon" ]
}

---

[ManifestVersion](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Paths/Components/ManifestVersion.cs) can only be executed in the context of a Pipeline with an assigned Manifest.

## Context

* Can only be used in PipelineJobs that execute on Manifests or in ManifestDatum StagingPaths

## Remarks

When executed within the context of a Pipeline with an assigned Manifest, the ManifestVersion component will retrieve and return the value of the Manifest's ManifestIdentity.Version
