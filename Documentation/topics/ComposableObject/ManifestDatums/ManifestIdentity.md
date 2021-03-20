The Manifest Identity stores unique identifying information used by ThunderKit to construct dependency information for stores and mod loaders.

![ManifestIdentity](Packages/com.passivepicasso.thunderkit/Documentation/graphics/ManifestIdentity.png)

### Required Fields
  * Author: The name of the developer or team responsible for developing and releasing this package.
  * Name: The name of this package, this is the dependency name and can only contain valid path characters and except for spaces
  * Version: The current in development version of this pacakge.

The fields specified by the Manifest Identity are available to be used by Pipeline Jobs and Path References to construct and find whatever information they may need to complete their execution.

### Dependencies

The ManifestIdentity contains the dependencies for each mod.
Many PipelineJobs will use the dependency hierarchy to discover all assets that need to be built or copied to Staging Paths.