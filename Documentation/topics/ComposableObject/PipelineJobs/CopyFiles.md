Use [CopyFiles](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Pipelines/Jobs/CopyFiles.cs) in a Pipeline to deploy content from Staging

When Per Maniest is toggled CopyFiles will execute once for each manifest in the Manifest hierarchy.
If Per Manifest is not toggled, no Manifest information will be available for PathReferences.

Use the Excluded Manifests fields to prevent this job from executing against a manifest when Per Manifest is toggled.

Both the Source File and the Destination File fields are PathReference compatible.

For example if you use [ManifestPluginStaging](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Templates/PathReferences/ManifestPluginStaging.asset) in StagingPaths in your Manifest's ManifestDatums
You could then use [ManifestPluginStaging](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Templates/PathReferences/ManifestPluginStaging.asset) in CopyFiles with Per Manifest toggled to copy those files to another location

This way you can deploy assets from multiple Manifests in your project simultaneously.

However, if the Per Manifest option is not toggled, an error will occur when using those PathReferences as they utilize information from Manifests to complete their task

The [BepInEx Launch Pipeline](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Templates/BepInEx/Pipelines/Launch.asset) uses CopyFiles to copy a the winhttp.dll file into the Game root directory so that BepInEx can conduct its startup operations.

![CopyFiles](Packages/com.passivepicasso.thunderkit/Documentation/graphics/PipelineJobs/CopyFiles.png)