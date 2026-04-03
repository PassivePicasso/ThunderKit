---
{ 
	"title" : "Import Addressable Catalog",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/3bb24ae4d588a7b4fbc1757e2fb5fd78){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[ImportAddressableCatalog](documentation://GUID/84c7a73e402727b41ac40b5245504aad)

Copies the Addressable Asset System catalog and settings files from the game
into the Unity project. Only relevant for games that use Unity's Addressable
Asset System.

This extension executes at `0` priority (`Constants.Priority.AddressableCatalog`).

## What It Does

Copies `catalog.json` and `settings.json` from the game's addressable assets
folder (`{GameData}/StreamingAssets/aa/`) to `Assets/StreamingAssets/aa/` in the
project. If a previous copy exists at the destination it is overwritten.

If either source file does not exist in the game the extension silently skips
the copy and returns without error.

## Scripting Define

On a successful copy the `TK_ADDRESSABLE` scripting define symbol is added to
the project. This allows conditional compilation using `#if TK_ADDRESSABLE`
directives.

## Requirements

Part of the **ThunderKit.Addressable** assembly. This extension is only
discovered if the ThunderKit.Addressable assembly is present and its
AssemblyInfo includes `[assembly: ImportExtensions]`.
