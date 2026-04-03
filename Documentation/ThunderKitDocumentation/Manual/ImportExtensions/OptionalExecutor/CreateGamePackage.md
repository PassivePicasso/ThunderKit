---
{ 
	"title" : "Create Game Package",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/a4e66fd1b2f0a6b4e951af502eba5a2b){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[CreateGamePackage](documentation://GUID/c72319cdfed39d34caab9a31e63e23ad)

Creates a `package.json` file for the game so that Unity's Package Manager
recognizes it as a local package. The package appears under
`Packages/{GameName}/` and makes the game's imported assemblies available as
project references.

This extension executes at `1,000,000` priority (`Constants.Priority.CreateGamePackage`).

## What It Creates

A package manifest containing:
- **Name** derived from the game executable filename
- **Display name** matching the game executable filename
- **Author** from `PlayerSettings.companyName`
- **Version** from `Application.version`
- **Description** indicating imported assemblies from the game

## Scripting Define

Adds a scripting define symbol matching the package name (e.g. the executable
name without extension). This allows conditional compilation using
`#if {GameName}` directives in your code.

## Package Resolution

After creating the manifest, triggers Unity's package resolution so the new
package is recognized immediately without requiring a manual refresh.
