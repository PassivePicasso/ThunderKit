---
{
	"title" : "About ThunderKit",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Server_Icon" ]
}

---

ThunderKit is a mod development environment extension for the Unity Editor with a wide variety of features.

ThunderKit is designed for modders, and to help support that it has been developed with highly extensible features that minimize the effort required to add functionality to your ThunderKit environment and share them with the communities you love!

Feature List:

* Manifests and ManifestDatums allow you to specify exactly what you need for your mods, from asstes like Meshes and Textures or ScriptableObjects and Prefabs, but also extra information such as versions and names for your mods. If an existing ManifestDatum doesn't provide the information you need, easily make a new one from the Add ManifestDatum menu!

* PathReferences and PathComponents allow you to create simple systems of computed paths so you can centrally manage paths, making maintenance changes easy by allow you to change things in one place.  PathReferences can be used in ManifestDatums as well as PipelineJobs by referencing them in path fields using arrow brackets ``` <GamePath>/mods  ```

* Pipelines and PipelineJobs use the asset listings and other information provided by Manifests and ManifestDatums to build and deploy your mods how you need. Construct custom Pipelines that do only what you need or utilize templates created by others to speed up the process.  Share your Pipelines with other modders to help build easy to re-use systems for games you like to mod!

* Documentation with ThunderKit is easy to use with DocumentationRoots and Markdown files.  Markdown in directories with and subdirectories of DocumentationRoots will be added to this [Documentation Window](menulink://Tools/ThunderKit/Documentation). Share your documentation with others by publishing them with your mods, or uploading them separately as packages to the [ThunderKit Extensions Thunderstore](https://thunderkit.thunderstore.io/)

* Want to add support for mod.io, moddb, steam workshop, or other store fronts? Add new sources like this for the ThunderKit Package Manager by implementing a new PackageSource.

* Share extensions to the ThunderKit environment using [ThunderKit Extensions Thunderstore](https://thunderkit.thunderstore.io/) helping to jumpstart your community members modding projects.

