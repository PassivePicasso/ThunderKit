---
{
	"pageStylePath" : "Packages/com.passivepicasso.thunderkit/uss/documentation_style.uss",
	"headerClasses" : [ ],
	"titleClasses" : [ ],
	"iconClasses" : [ ]
}

---

# Manifest {#manifest .page-header-container .header-icon-manifest }

Manifests are where you will store all the information about your projects for ThunderKit to utilize. This includes meta data information for mod distribution systems like Thunderstore. Manifests are composed of ManifestDatums, there are many ManifestDatums already available. Check the ManifestDatums section for a list of ManifestDatums and their functionality.

All Manifests are prepopulated with a ManifestIdentity. The ManifestIdentity is where information about the identity of a mod and what dependencies it has are stored.  You can drag and drop any Manifest into the ManifestIdentity's Dependencies field, or access it from the Unity asset finder by clicking the small circle.

ManifestDatums all have an array named Staging Paths.  This array of strings informs Pipelines where the information is expected to be written out to. As previously mentioned, the StagingPaths array can utilize PathReferences by invoking one with the arrow bracket operators <>.

If you would like to check out an example, inspect the [Default-BepInEx](assetlink://GUID/bc5e6d3336544e5361d16e63ddfca327) manifest in the ThunderKit Package.