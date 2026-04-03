---
{ 
	"title" : "Disable Assembly Updater",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/208b77589c09e314aa3dbfae9117393a){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[DisableAssemblyUpdater](documentation://GUID/d8fd47d44859b0d4d9b38ca56a15980f)

Checks whether Unity was launched with the `-disable-assembly-updater` command
line argument and prompts the user to restart if it was not.

This extension executes at `int.MaxValue - 100` priority (runs second, after
Check Unity Version).

## Why Disable the Assembly Updater?

Unity's Assembly Updater automatically rewrites managed assemblies to fix API
changes between Unity versions. For modding, the game's assemblies should remain
unmodified. Disabling the updater also reduces import times significantly.

If the import process seems to never end the fix is usually to disable the
Assembly Updater.

## What Happens

If the `-disable-assembly-updater` flag is already present this extension does
nothing and returns immediately.

If the flag is missing a dialog is displayed with two options:

- **Restart Project** -- Restarts Unity with the `-disable-assembly-updater`
  flag. The import will resume from the beginning on the next editor launch.
- **No Thanks** -- Continues the import without the flag. The game's assemblies
  may be modified by Unity's updater, which can cause issues.
