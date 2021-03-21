PathReferences are ComposableObjects composed of PathComponents to compute paths,  use them to coordinate the deployment of files for your Manifests and Pipelines.

If existing PathComponents do not allow you to describe the path you need, create a new PathComponent using the ComposableObject Inspector to fill in the missing steps.

ThunderKit comes with a number of already defined PathReferences for the BepInEx workflow.

Please note that some adjustments may be needed for different games.

  * [Common PathReference Assets](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Templates/PathReferences)

  * [BepInEx PathReference Assets](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Templates/BepInEx/PathReferences)

PathReferences can be used in a number of places in ThunderKit. ManifestDatum.StagingPaths, StageAssetBundles.BundleArtifactPath and all the Copy PipelineJobs allow you to use PathReferences by calling them in a tag.

For Example

`
<ManifestPluginStaging>/images
`
