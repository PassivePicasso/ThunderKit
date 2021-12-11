---
{ 
	"pageStylePath" : "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"contentClasses" : [ "page-header" ],
	"iconUrl" : "Packages/com.passivepicasso.thunderkit/Documentation/graphics/TK_PathReference_2X_Icon.png",
	"iconClasses" : [ "header-icon" ]
}

---

[RegistryLookup](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Paths/Components/RegistryLookup.cs) will return the value at the specified value from the specified key

## Fields

* **Key Name**
  - Option to search only top directory or all sub directories as well
* **Value Name**
  - pattern to search for in file and folder names

## Remarks

Acquire a value from the windows registry.

This can be used to locate applications to execute that register their installation location with the windows registry.