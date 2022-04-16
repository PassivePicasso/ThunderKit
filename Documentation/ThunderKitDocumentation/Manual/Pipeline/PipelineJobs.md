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

1. ManifestDatum [AssemblyDefinitions](documentation://GUID/cef5acb6a795c5d4d9031261ea82e891) and PipelineJob [StageAssemblies](documentation://GUID/c18be5deeb5af6d48b59ed26056ba2fc)
2. ManifestDatum [AssetBundleDefinitions](documentation://GUID/b3d3f798ec15f8240ad5105c46ce59f5) and PipelineJob [StageAssetBundles](documentation://GUID/346cbbd3f6c582441908249cf4067307)
3. ManifestDatum [UnityPackages](documentation://GUID/e3fa281f0f933e64480e9eecdf057350) and PipelineJob [StageUnityPackages](documentation://GUID/915bcab14ba398e48b20a17d5e79b13b)

Examine these pairs of types to gain a better understanding of how to build your own customized set of data.

See each ManifestDatums page for more information about them.