---
{ 
	"title" : "Unity Package Installer",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/213e13d5b2469964d921c60eadde042c){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[UnityPackageInstaller](documentation://GUID/03891ed5d95f7ab48886fac5c76769b2)

Abstract base class for installing Unity packages through the Unity Package
Manager as part of the import process. The `Execute` method is sealed.

This class is abstract and does not appear on the Import Configuration page by
itself. Create a concrete subclass to install a specific package.

## Abstract Properties

* **PackageIdentifier** (`string`)
  The package identifier to install. This can be a registry package name
  (e.g. `com.unity.addressables`), a git URL, or any identifier accepted by
  `UnityEditor.PackageManager.Client.Add()`.

## How It Works

Calls `Client.Add()` with the PackageIdentifier, waits for the request to
complete, then triggers package resolution so the newly installed package is
available immediately.

## Example

```cs
using ThunderKit.Core.Config.Common;

public class InstallAddressables : UnityPackageInstaller
{
    public override int Priority => 500_000;

    public override string PackageIdentifier => "com.unity.addressables";
}
```
