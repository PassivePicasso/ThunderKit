---
{ 
	"title" : "Managed Assembly Patcher",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/c0960d561d36deb4aac684c83e4f0e74){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[ManagedAssemblyPatcher](documentation://GUID/ce92779cb49e6bb448fd6987a24d4296)

Abstract base class for applying binary patches to game assemblies during
import. Uses the BsDiff algorithm to produce a patched copy of an assembly and
writes it to the game's package folder.

This class is abstract and does not appear on the Import Configuration page by
itself. Create a concrete subclass to patch a specific assembly.

## Abstract Properties

* **AssemblyFileName** (`string`)
  The filename of the assembly to patch (e.g. `Assembly-CSharp.dll`). The
  original file is read from the game's `Managed` folder.

* **BsDiffPatchPath** (`string`)
  Path to the BsDiff patch file that will be applied against the original
  assembly.

## How It Works

1. Reads the original assembly from the game's Managed folder.
2. Applies the BsDiff patch to produce a new assembly.
3. Writes the patched assembly and a `.meta` file to the game's package folder
   at `Packages/{GameName}/`.
4. If a previous copy of the patched assembly exists in the destination it is
   deleted first.

## Example

```cs
using ThunderKit.Core.Config;

public class PatchExampleAssembly : ManagedAssemblyPatcher
{
    public override int Priority => 2_500_000;

    public override string AssemblyFileName => "Assembly-CSharp.dll";

    public override string BsDiffPatchPath =>
        "Packages/com.example.mypatcher/Patches/Assembly-CSharp.patch";
}
```
