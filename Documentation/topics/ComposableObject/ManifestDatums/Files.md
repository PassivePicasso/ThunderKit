The [AssemblyDefinitions](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Manifests/Datum/AssemblyDefinitions.cs) ManifestDatum stores references to Unity AssemblyDefinition objects.

## Fields
* **Files**
  - An array of assets

## Inherited Fields

* **Staging Paths**
  - A list of destinations to deploy files to
  - Supports PathReferences

## PipelineJobs

* [StageManifestFiles](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Pipelines/Jobs/StageManifestFiles.cs) 
  - Copies each asset referenced in each Files ManifestDatum to the output paths defined in its Staging Paths.

## Remarks

Use this ManifestDatum to specify and group files to be deployed by Pipelines using the [StageManifestFiles](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Pipelines/Jobs/StageManifestFiles.cs) PipelineJob