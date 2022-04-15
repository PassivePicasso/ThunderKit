---
{ 
	"title" : "ManifestDatums",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Manifest_2X_Icon" ]
}

---

ManifestDatums are where collections of information can be stored.
ThunderKit comes with a few ManifestDatums to cover common use cases, however if you find that they do not cover your case you should create a ManifestDatum to 
collect the information you need.

ManifestDatums and PipelineJobs are paired together in ThunderKit.  When you create a new ManifestDatum you will need to create a PipelineJob to consume it.

Some pairs that already exist are;

1. ManifestDatum [AssemblyDefinitions](assetlink://GUID/2b7e13dda513544419a89926bd12ad8a) and PipelineJob [StageAssemblies](assetlink://GUID/b5b20fac9c71fd64183cb7a8f359d73a)
2. ManifestDatum [AssetBundleDefinitions](assetlink://GUID/17d1008b78cb6e846889b7778282fbef) and PipelineJob [StageAssetBundles](assetlink://GUID/924ee63e6c016f14d8a1560b288f15a3)
3. ManifestDatum [UnityPackages](assetlink://GUID/dda4ac7962f04724eacfeb26af5e2b75) and PipelineJob [StageUnityPackages](assetlink://GUID/d087870ea8abaed4ca4c717444be0165)

Examine these pairs of types to gain a better understanding of how to build your own customized set of data.

See each ManifestDatums page for more information about them.