---
title: "ThunderKit Introduction"
weight: 1
---

# Pillars 


ThunderKit is built upon a small set of core features that are built around 3 core pillars.
{{< block "grid-3" >}}

{{< column >}}
#### Flexibility
Provide features that have the ability to fill many needs and roles in a project, so to maximize the utility of its implementation.

This maximization helps focus the project on having a small amount of code with simple interaction.
{{< /column >}}

{{< column >}}
#### Usability 
Ensure that each feature is simple, understandable, and has a clear place in the system.

This way authors can focus on expressing their intent rather than fighting with how the system solves problems.
{{< /column >}}

{{< column >}}
#### Extensibility 
To ensure that ThunderKit allows you to reach your objectives.

If a feature doesn't provide the utility you need, it should be easy to add on the functionality you need.
{{< /column >}}

{{< /block >}}

With these ideals in mind ThunderKit has 3 core systems, [PathReferences](#pathreferences), [Manifests](#manifests), and [Pipelines](#pipelines)

These 3 systems are all built upon a ComposableObject, ComposableObjects are how they sound, they are just buckets for a number of scripts that belong together in some way.
A ComposableObject has a type of component it can host, and it can setup code to process its components if needed.

PathReferences consist of PathComponents
Manifests consist of ManifestDatums
Pipelines consist of PipelineJobs

### PathReferences

PathReferences centralize management of files and folders in your projects.
These will primarily be used in complex projects with customized pipelines, or to setup new integration templates for modding frameworks.
ThunderKit's core set of scripts are setup to resolve references to PathReferences automatically.

When entering a path to a file or folder, you can include a reference to a PathReference 
For Example, say you have a PathReference you named MyAssets and you added Constant PathComponent to it with a value of Assets
{{< block "grid-2" >}}
{{< column >}}
If you use this path

    <MyAssets>/Textures/ThunderKit.png
{{< /column >}}
{{< column >}}
It will resolve to this path

    Assets/Textures/ThunderKit.png
{{< /column >}}
{{< /block >}}

Many PathReferences are already created for your convenience and can be referenced in this way. 

Currently available core PathReferences are
{{< block "grid-3" >}}
{{< column >}}
&lt;AssetBundleStaging&gt;

&lt;Pwd&gt;

&lt;StagingRoot&gt;
{{< /column >}}
{{< column >}}
&lt;ManifestPluginStaging&gt;

&lt;ManifestStagingRoot&gt;
{{< /column >}}
{{< column >}}
&lt;GameExecutable&gt;

&lt;GamePath&gt;
{{< /column >}}
{{< /block >}}

### Manifests

Manifests are where you list assets, files, scripts and other pieces of information that need to be destributed with your mod and processed by Pipelines

### Pipelines


Pipelines are an extensible system that executes steps that can utilize information from Manifests