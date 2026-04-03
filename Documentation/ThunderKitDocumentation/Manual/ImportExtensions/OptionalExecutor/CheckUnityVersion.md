---
{ 
	"title" : "Check Unity Version",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/8dcb1359c6da7c049b1063e3561a1ecf){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[CheckUnityVersion](documentation://GUID/cbec789a5a4890b41844efe9f2069e31)

Validates that the Unity Editor version matches the version of Unity used to
build the game. If the versions do not match this extension throws an exception
and halts the import process.

This extension executes at `int.MaxValue - 50` priority (highest among all
default import extensions -- runs first).

## How It Works

1. Reads the game's Unity version from the `globalgamemanagers` file (or
   `data.unity3d`) using AssetsTools.NET.
2. If that file cannot be parsed, falls back to reading the `FileVersionInfo`
   from the game executable.
3. Compares the `major.minor.patch` portion of each version string against
   `Application.unityVersion`.

## What Happens on Mismatch

Throws an exception displaying both version strings and a message instructing
the user to switch to the matching Unity Editor version. The import process is
halted -- no further import extensions run.

## Disabling This Check

This check can be disabled on the Import Configuration settings page. Importing
a game with an unmatched version of Unity is unsupported and may cause
unpredictable project behavior. The option is available for debugging only.
