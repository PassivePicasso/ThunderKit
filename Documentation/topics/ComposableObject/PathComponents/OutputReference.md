---
{ 
	"pageStylePath" : "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"contentClasses" : [ "page-header" ],
	"iconUrl" : "Packages/com.passivepicasso.thunderkit/Documentation/graphics/TK_PathReference_2X_Icon.png",
	"iconClasses" : [ "header-icon" ]
}

---

[OutputReference](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Paths/Components/OutputReference.cs) allows you to reference another PathReference using Unity's Asset system.

## Fields
* **Reference**
  - PathReference to call upon with the context of the parent PathReference

## Remarks

These can be used to build more complex PathReferences by combining smaller ones

Use this over a Resolver is you want to ensure that references are maintained through name changes
