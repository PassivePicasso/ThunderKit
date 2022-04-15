---
{ 
	"title" : "PipelineJobs",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Pipeline_2X_Icon" ]
}

---

PipelineJobs are what conduct Unity Build actions such as staging assets, assetbundles and other files, copying and moving files for deployment, as well as creating and modifying zip files.

ThunderKit comes with a collection of PipelineJobs that cover each of these use cases and like ManifestDatums and PathComponents you can create new PipelineJobs using the help of ComposableObject Inspector.

PipelineJobs and ManifestDatums are paired together in ThunderKit.  When you create a new PipelineJob you may need to create a ManifestDatum to hold information for it.

Some pairs that already exist are;

1. ManifestDatum [AssemblyDefinitions](assetlink://GUID/2b7e13dda513544419a89926bd12ad8a) and PipelineJob [StageAssemblies](assetlink://GUID/b5b20fac9c71fd64183cb7a8f359d73a) 
2. ManifestDatum [AssetBundleDefinitions](assetlink://GUID/17d1008b78cb6e846889b7778282fbef) and PipelineJob [StageAssetBundles](assetlink://GUID/924ee63e6c016f14d8a1560b288f15a3) 
3. ManifestDatum [UnityPackages](assetlink://GUID/dda4ac7962f04724eacfeb26af5e2b75) and PipelineJob [StageUnityPackages](assetlink://GUID/d087870ea8abaed4ca4c717444be0165) 

Examine these pairs of types to gain a better understanding of how to build your own customized set of data.

See each ManifestDatums page for more information about them.