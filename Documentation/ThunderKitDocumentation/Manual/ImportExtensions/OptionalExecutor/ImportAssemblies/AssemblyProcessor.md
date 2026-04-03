---
{ 
	"title" : "AssemblyProcessor",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/bb6bf1bc7a41a0f48a824094fcd4255a){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[ImportAssemblies](documentation://GUID/b216ba4bf77cd2b4eacfed464cc6540e) >
[AssemblyProcessor](documentation://GUID/4a190cef70983534c9b13cf066c986ae)

Abstract base class for redirecting assembly import paths during
[ImportAssemblies](documentation://GUID/b216ba4bf77cd2b4eacfed464cc6540e).
Inherits from `ImportExtension` (not `OptionalExecutor`).

Override the `Process` method to change where a specific assembly is imported
from. For example, redirect an assembly to a pre-modified copy stored in a
different location.

## Virtual Method

* **Process** (`string path`) returns `string`
  Receives the current file path of an assembly being imported. Return a new
  path to import from that location instead, or return the path unchanged to
  leave it as-is. The default implementation returns the path unchanged.

## Discovery

AssemblyProcessors are discovered at editor load time from assemblies marked
with `[assembly: ImportExtensions]`. Multiple processors run in Priority order
(descending) and each receives the output path of the previous processor.

## Example

```cs
using ThunderKit.Core.Config;

public class RedirectAssembly : AssemblyProcessor
{
    public override int Priority => 0;

    public override string Process(string path)
    {
        if (path.EndsWith("MyAssembly.dll"))
            return "Assets/ModifiedAssemblies/MyAssembly.dll";
        return path;
    }
}
```
