---
{ 
	"title" : "BlacklistProcessor",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/c125ac53637cba343a5ce08c0895b4f8){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[ImportAssemblies](documentation://GUID/b216ba4bf77cd2b4eacfed464cc6540e) >
[BlacklistProcessor](documentation://GUID/8fc5513479ffdf544bcdedc3f58edb1c)

Abstract base class for modifying the assembly blacklist during
[ImportAssemblies](documentation://GUID/b216ba4bf77cd2b4eacfed464cc6540e).
Inherits from `ImportExtension` (not `OptionalExecutor`).

The blacklist determines which assemblies are skipped during import. By default
it contains all assemblies currently loaded in the Unity Editor domain, which
prevents importing Unity's own DLLs and editor assemblies.

## Abstract Method

* **Process** (`IEnumerable<string> blacklist`) returns `IEnumerable<string>`
  Receives the current blacklist of assembly filenames. Return a modified
  enumerable to add or remove entries. Can filter, append, or replace entries
  as needed.

## Blacklist vs Whitelist

The blacklist is overridden by the whitelist. If an assembly filename appears on
both lists it IS imported. See
[WhitelistProcessor](documentation://GUID/7c18e22efc965c6408abb5ab47cd04aa)
for the counterpart.

## Discovery

BlacklistProcessors are discovered at editor load time from assemblies marked
with `[assembly: ImportExtensions]` and run in Priority order (descending).

## Example

```cs
using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Config;

public class RemoveFromBlacklist : BlacklistProcessor
{
    public override int Priority => 0;

    public override IEnumerable<string> Process(IEnumerable<string> blacklist)
    {
        // Allow MySpecialAssembly.dll to be imported even though
        // it is loaded in the editor
        return blacklist.Where(name => name != "MySpecialAssembly.dll");
    }
}
```
