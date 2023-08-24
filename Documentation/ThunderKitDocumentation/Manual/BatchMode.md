---
{
	"title" : "BatchMode (Command line execution)",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Documentation_2X_Icon" ]
}

---



## Arguments

* **Pipeline**
  -  `-pipeline=path `
      - Project Relative Path to target Pipeline 
  - ```-manifest=path```
      - Project Relative Path to target Manifest
  - ```-show-log-window```
      - Enables displaying the Pipeline Log window during execution
 
 ## Remarks
 
Batchmode enables the Execution of Pipelines and Manifests by invoking 
pre-constructed pipelines.  This can be useful for complex processing jobs or
integration with external development and build tools.

To use this feature you will need to execute the appropriate version of Unity
and supply atleast the -projectPath, -executeMethod and -pipeline
arguments, and optionally the -batchmode and -manifest arguments.
Using these command line arguments you can execute ThunderKit Pipelines.

To learn more about Unity's command line arguments refer to
[Unity Editor command line arguments](https://docs.unity3d.com/Manual/EditorCommandLineArguments.html)

To execute the pipeline you must call the execution method. 

You can do this using the Unity Command line argument -executeMethod and
providing ThunderKit.Core.Pipelines.Pipeline.BatchModeExecutePipeline next.

The following demonstrates a complete setup

```
C:\Program Files\Unity\Hub\Editor\2019.4.26f1\Editor\Unity.exe" -batchmode 
-projectpath F:\Projects\Unity\ExampleProject
-manifest="Assets/ThunderKitAssets/BundleManifest.asset"
-pipeline="Assets/ThunderKitAssets/BuildBundles.asset"
-executeMethod ThunderKit.Core.Pipelines.Pipeline.BatchModeExecutePipeline  
```