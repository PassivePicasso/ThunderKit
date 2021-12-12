---
{ 
	"pageStylePath" : "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconUrl" : "Packages/com.passivepicasso.thunderkit/Documentation/graphics/TK_PathReference_2X_Icon.png",
	"iconClasses" : [ "header-icon" ]
}

---

[WorkingDirectory](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Paths/Components/WorkingDirectory.cs) will return the current Process Working Directory

## Remarks

Unity sets the working directory to the root folder of the active project, so this should always return the same value within a project

If you use the Execute Process pipeline, you should take into consideration that the new process will not implicitly inherit the Working directory of the Unity project and you may need to set it explicitly.
