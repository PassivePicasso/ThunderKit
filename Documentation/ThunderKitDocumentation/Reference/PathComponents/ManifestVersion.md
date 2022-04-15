---
{ 
	"title" : "ManifestVersion",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_PathReference_2X_Icon" ]
}

---

[ManifestVersion](assetlink://GUID/41b82db3744610d4ba6a7edd9df00836) can only be executed in the context of a Pipeline with an assigned Manifest.

## Context

* Can only be used in PipelineJobs that execute on Manifests or in ManifestDatum StagingPaths

## Remarks

When executed within the context of a Pipeline with an assigned Manifest, the ManifestVersion component will retrieve and return the value of the Manifest's ManifestIdentity.Version
