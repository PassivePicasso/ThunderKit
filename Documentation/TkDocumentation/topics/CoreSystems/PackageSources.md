---
{ 
	"pageStylePath" : "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss",
	"iconUrl" : "Packages/com.passivepicasso.thunderkit/Documentation/graphics/TK_PackageSource_2X_Icon.png",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon" ]
}

---

PackageSources provide the ThunderKit PackageManager with package listings that can be downloaded and installed into a ThunderKit Project.
You can manage your PackageSources from [ThunderKit Settings](menulink://Tools/ThunderKit/Settings) where you can Add, Remove and Refresh your PackageSources.

Currently, ThunderKit provides two PackageSources, ThunderstoreSource and LocalThunderstoreSource as part of the Thunderstore integration for ThunderKit.

Additional sources can be developed to support other stores, see the source code for [ThunderstoreSource](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Integrations/Thunderstore/ThunderstoreSource.cs) and [LocalThunderstoreSource](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Integrations/Thunderstore/LocalThunderstoreSource.cs) for some examples.
