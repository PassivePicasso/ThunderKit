---
{ 
		"title" : "BepInEx Local Configuration Guide",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Documentation_2X_Icon" ]
}

---


This guide will inform you how to setup a zipped copy of BepInEx configured to be compatible with Thunderstore and additionally demonstrate how you can keep a collection of packages on your local machine.

After completing this guide you should have the tools you need to:
* Create a new Package for Thunderstore manually
* Setup BepInEx for a game without a preconfigured BepInEx Package on Thunderstore

## Download BepInEx

The first thing you should do is download a copy of BepInEx that is compatible with the game you're modding.

Refer to the BepInEx Documentation, [Installing BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) for details on compatibility and dowlnoading BepInEx.

## Create Local Thunderstore Source

Start by creating a LocalThunderstoreSource under in [Settings window](menulink://Tools/ThunderKit/Settings) by clicking the Add button under the PackageSourceSettings section and selecting LocalThunderstoreSource

![](Packages/com.passivepicasso.thunderkit/Documentation/graphics/Tutorials/NewGameBepInEx/NewLocalThunderstoreSource.png)