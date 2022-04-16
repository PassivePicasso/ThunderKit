---
{ 
	"title" : "WorkingDirectory",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_PathReference_2X_Icon" ]
}

---

[WorkingDirectory](assetlink://GUID/82f9860b00305774b85c0ef6b5e4b0d1) will return the current Process Working Directory

## Remarks

Unity sets the working directory to the root folder of the active project, so this should always return the same value within a project

If you use the Execute Process pipeline, you should take into consideration that the new process will not implicitly inherit the Working directory of the Unity project and you may need to set it explicitly.
