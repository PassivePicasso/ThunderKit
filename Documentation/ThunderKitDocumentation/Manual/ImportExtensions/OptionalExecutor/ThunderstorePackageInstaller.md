---
{ 
	"title" : "Thunderstore Package Installer",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/f1e1d03284e715444951e6ec9eb21e73){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[ThunderstorePackageInstaller](documentation://GUID/ca5cdbe1c4ee63646b3515f5995776b0)

Abstract base class for automatically installing Thunderstore packages during
the game import process. The `Execute` method is sealed.

This class is abstract and does not appear on the Import Configuration page by
itself. Create a concrete subclass to install a specific Thunderstore package.

## Abstract Properties

* **DependencyId** (`string`)
  The Thunderstore dependency ID of the package to install
  (e.g. `AuthorName-PackageName-1.0.0`).

* **ThunderstoreAddress** (`string`)
  The URL of the Thunderstore community API
  (e.g. `https://thunderstore.io/c/risk-of-rain-2/`).

## Optional Properties

* **ForceLatestDependencies** (`bool`, default `false`)
  When `true`, forces the latest version of all transitive dependencies to be
  installed regardless of the version specified in the dependency string.

## How It Works

1. Looks for an existing `ThunderstoreSource` in the project's Package Source
   settings that matches the ThunderstoreAddress URL.
2. If none is found, creates a temporary (transient) `ThunderstoreSource` and
   loads its package listing.
3. Locates the package by DependencyId and calls `InstallPackage`.
4. Assembly reload is locked during the installation to prevent interruption.
5. If the package is already installed the extension logs a message and skips.
6. Returns `false` (retry) if the package source has no packages or the
   DependencyId cannot be found.

The transient source created during installation is destroyed in the `Cleanup`
phase after all import extensions have finished.

## Requirements

Part of the **ThunderKit.Thunderstore** assembly. Requires the
ThunderKit.Thunderstore assembly to be present and its AssemblyInfo to include
`[assembly: ImportExtensions]`.

## Example

```cs
using ThunderKit.Integrations.Thunderstore;

public class InstallBepInEx : ThunderstorePackageInstaller
{
    public override int Priority => 800_000;

    public override string DependencyId =>
        "bbepis-BepInExPack-5.4.2100";

    public override string ThunderstoreAddress =>
        "https://thunderstore.io/c/risk-of-rain-2/";
}
```
