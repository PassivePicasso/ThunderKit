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

1. ManifestDatum [AssemblyDefinitions](documentation://GUID/cef5acb6a795c5d4d9031261ea82e891) and PipelineJob [StageAssemblies](documentation://GUID/c18be5deeb5af6d48b59ed26056ba2fc)
2. ManifestDatum [AssetBundleDefinitions](documentation://GUID/b3d3f798ec15f8240ad5105c46ce59f5) and PipelineJob [StageAssetBundles](documentation://GUID/346cbbd3f6c582441908249cf4067307)
3. ManifestDatum [UnityPackages](documentation://GUID/e3fa281f0f933e64480e9eecdf057350) and PipelineJob [StageUnityPackages](documentation://GUID/915bcab14ba398e48b20a17d5e79b13b)

Examine these pairs of types to gain a better understanding of how to build your own customized set of data.

See each ManifestDatums page for more information about them.