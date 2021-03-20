Starting with Unity 2018.1 Unity began to provide the ability to organize scripts into Assemblies using AssemblyDefinitions.


ThunderKit makes use of Unity's AssemblyDefinitions, allowing you to control how you want to organize your code. To include code with a mod you will need to add the ManifestDatum AssemblyDefinitions to your Manifest.

![AssemblyDefinitions](Packages/com.passivepicasso.thunderkit/Documentation/graphics/AssemblyDefinitions.png)

The PipelineJob StageAssemblies will copy each assembly referenced in each AssemblyDefinition ManifestDatum to the output paths defined in its Staging Paths.

#### More Information

[Unity Manual - Script compilation and assembly definition files](https://docs.unity3d.com/2018.4/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html)
