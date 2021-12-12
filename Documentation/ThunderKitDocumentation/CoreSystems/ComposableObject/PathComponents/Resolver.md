---
{ 
	"pageStylePath" : "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_PathReference_2X_Icon" ]
}

---

[Resolver](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Paths/Components/Resolver.cs) takes a string that it will resolve using the PathReference system.

## Fields
* **Value**
  - String path that can include PathReference tags
  - Supports PathReferences

## Remarks

Use PathReference tags in the Value field in combination with literal string values to build complex paths
