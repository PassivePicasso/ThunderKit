---
{ 
	"title" : "Unity Package Uninstaller",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/469f8ad306016a44e877a98c0db1d815){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[UnityPackageUninstaller](documentation://GUID/741f8e5d5c63e5640bbf7c9334a597a9)

Abstract base class for removing Unity packages through the Unity Package
Manager as part of the import process. The `Execute` method is sealed.

This class is abstract and does not appear on the Import Configuration page by
itself. Create a concrete subclass to remove a specific package.

## Abstract Properties

* **PackageIdentifier** (`string`)
  The package identifier to remove (e.g. `com.unity.addressables`).

## How It Works

Calls `Client.Remove()` with the PackageIdentifier, waits for the request to
complete, then triggers package resolution.

## Example

```cs
using ThunderKit.Core.Config.Common;

public class UninstallTextMeshPro : UnityPackageUninstaller
{
    public override int Priority => 500_000;

    public override string PackageIdentifier => "com.unity.textmeshpro";
}
```
