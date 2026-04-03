---
{ 
	"title" : "WhitelistProcessor",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/ed18265f87e7ac14e85fc91ff8183a70){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[ImportAssemblies](documentation://GUID/b216ba4bf77cd2b4eacfed464cc6540e) >
[WhitelistProcessor](documentation://GUID/7c18e22efc965c6408abb5ab47cd04aa)

Abstract base class for modifying the assembly whitelist during
[ImportAssemblies](documentation://GUID/b216ba4bf77cd2b4eacfed464cc6540e).
Inherits from `ImportExtension` (not `OptionalExecutor`).

The whitelist determines which assemblies are always imported regardless of the
blacklist. By default it contains all files already present in the package
destination folder, preserving assemblies from a previous import.

## Abstract Method

* **Process** (`IEnumerable<string> whitelist`) returns `IEnumerable<string>`
  Receives the current whitelist of assembly filenames. Return a modified
  enumerable to add or remove entries.

## Whitelist vs Blacklist

The whitelist overrides the blacklist. If an assembly filename appears on both
lists it IS imported. See
[BlacklistProcessor](documentation://GUID/8fc5513479ffdf544bcdedc3f58edb1c)
for the counterpart.

## Discovery

WhitelistProcessors are discovered at editor load time from assemblies marked
with `[assembly: ImportExtensions]` and run in Priority order (descending).

## Example

```cs
using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Config;

public class ForceImportAssembly : WhitelistProcessor
{
    public override int Priority => 0;

    public override IEnumerable<string> Process(IEnumerable<string> whitelist)
    {
        // Ensure this assembly is always imported even if it would
        // normally be blacklisted
        return whitelist.Append("RequiredPlugin.dll");
    }
}
```
