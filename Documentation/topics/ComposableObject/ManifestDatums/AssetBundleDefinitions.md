AssetBundleDefinitions allow you to specify what assets you would like to include in your project output in the form of a Unity Asset Bundle.

The PipelineJob, StageAssetBundles, will use the AssetBundleDefinitions ManifestDatum and the Pipeline Manifest's dependency hierarchy to determine how to build out the AssetBundles.
This means that only AssetBundles defined within the Pipelines Manifest hiearchy will be included in StageAssetBundles processing.

When StageAssetBundles executes, it will process all AssetBundles simultanously, and will resolve dependencies both within and across Manifests.

![AssetBundleDefinitions](Packages/com.passivepicasso.thunderkit/Documentation/graphics/ManifestDatums/AssetBundleDefinitions.png)

#### More Information

[Unity Manual - Asset Bundles](https://docs.unity3d.com/2018.4/Documentation/Manual/AssetBundlesIntro.html)