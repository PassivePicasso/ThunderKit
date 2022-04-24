---
{ 
	"title" : "PathComponents",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_PathReference_2X_Icon" ]
}

---

PathReferences are ComposableObjects composed of PathComponents to compute paths,  use them to coordinate the deployment of files for your Manifests and Pipelines.
When a PathReference is executed, each PathComponent that belongs to it is executed in order.
Each PathComponent will return a string for as a part of a path.

Once all PathComponents have returned the PathReference hands these values to System.IO.Path.Combine.

See the documentation for [Path.Combine](https://docs.microsoft.com/en-us/dotnet/api/system.io.path.combine?view=netframework-4.6) for rules about how Combine works.

If existing PathComponents do not allow you to describe the path you need, create a new PathComponent using the ComposableObject Inspector to fill in the missing steps.

ThunderKit comes with a number of already defined PathReferences for the BepInEx workflow.

Please note that some adjustments may be needed for different games.

  * [Common PathReference Assets](assetlink://GUID/8c6243a7bb8ce734ab8ae4ccf164bfb7)

  * [BepInEx PathReference Assets](assetlink://GUID/6733c4a0a9bdc9c44b9c486058325099)

PathReferences can be used in a number of places in ThunderKit. ManifestDatum.StagingPaths, StageAssetBundles.BundleArtifactPath and all the Copy PipelineJobs allow you to use PathReferences by calling them in a tag.

For Example

`
<ManifestPluginStaging>/images
`
