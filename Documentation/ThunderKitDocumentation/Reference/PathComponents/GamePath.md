---
{ 
	"title" : "GamePath",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_PathReference_2X_Icon" ]
}

---

[GamePath](assetlink://GUID/df39480b01ece8c459c9db887486877f) will return the value of GamePath in your [ThunderKitSettings](menulink://Tools/ThunderKit/Settings)

## Remarks

Use this to deploy files directly to the game folder.

This is used by the [BepInEx Launch Pipeline](assetlink://GUID/bee6483f5bcf7054b86d13321eef27e5) to deploy the winhttp.dll file to the games directory so that doorstop can intercept the application startup.