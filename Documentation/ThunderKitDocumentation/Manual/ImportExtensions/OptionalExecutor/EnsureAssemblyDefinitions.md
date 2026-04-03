---
{ 
	"title" : "Ensure Assembly Definitions",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/b7f32e9d1a0c584763d21af8c5b9e049){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[Ensure Assembly Definitions](documentation://GUID/c4d83e2fa1b7609857e4f82b3d6c0f15)

ThunderKit detected one or more C# scripts in your `Assets` folder that are
not covered by an Assembly Definition (`.asmdef`) file. The import has been
stopped to prevent build problems that are difficult to diagnose after the
fact.

This extension executes at `int.MaxValue - 150` priority.

## What is an Assembly Definition?

Unity compiles C# scripts into *assemblies* — bundles of compiled code. By
default, any script that is not explicitly organized ends up in a single
catch-all assembly called **Assembly-CSharp**.

An Assembly Definition file (`.asmdef`) tells Unity to compile everything in
and below a specific folder into its own named, isolated assembly instead.
This keeps your code cleanly separated from game code and from other mods.

- [Unity Manual — Assembly Definition files](https://docs.unity3d.com/Manual/assembly-definition-files.html)
- [Unity Manual — Assembly Definition Inspector reference](https://docs.unity3d.com/Manual/class-AssemblyDefinitionImporter.html)

## Why does this block the import?

When any C# script in your project is not covered by an `.asmdef`, Unity
compiles it into **Assembly-CSharp** and marks that assembly file as
**read-only**. ThunderKit needs to manage that file during game import, so
the read-only flag creates a conflict: the file gets regenerated and locked
again every time Unity recompiles, no matter how many times you clear it
manually or restart the project.

Removing the read-only flag yourself does not fix the underlying cause. As
long as an orphaned script exists, Unity will regenerate the file and lock it
again on the next recompile, blocking ThunderKit from importing the game's
assemblies into your project.

ThunderKit stops the import here so you can correct the project structure
before entering that loop.

## How to fix it — create a new Assembly Definition

This is the recommended path if you do not already have an `.asmdef` covering
the orphaned script. An `.asmdef` covers everything in its own folder **and
all subfolders below it**, so you can place it either next to the orphaned
file or in any parent folder above it.

1. Open the Unity [Project window](menulink://Window/General/Project).
2. Navigate to the folder that contains the orphaned `.cs` file, or to any
   parent folder that should own that script (for example, the root of all
   your mod scripts).
3. Right-click inside that folder and choose **Create > Assembly Definition**.
4. Unity creates a new `.asmdef` file. Give it a descriptive, unique name
   (for example, `MyMod` or `MyMod.Editor`).
5. Every `.cs` file in that folder and its subfolders is now managed by
   this new assembly — they will no longer be orphaned.
6. If the scripts are only needed inside the Unity Editor (tools, importers,
   custom inspectors), open the `.asmdef` in the Inspector, scroll to
   **Platforms**, and enable only **Editor**.
7. Re-run the import from `Tools > ThunderKit > Import Game`.

## How to fix it — move the script under an existing Assembly Definition

Use this path if you already have an `.asmdef` whose purpose matches the
script, and the script simply ended up in the wrong folder.

1. Locate an existing `.asmdef` file whose assembly is the right home for
   the script. Its folder appears in the [Project window](menulink://Window/General/Project)
   next to the `.asmdef` asset.
2. Move the orphaned `.cs` file(s) into that folder or any subfolder of it.
   Unity will recompile and the scripts will now belong to that assembly.
3. Re-run the import from `Tools > ThunderKit > Import Game`.

## How to find which files are orphaned

The error message printed to the [Console](menulink://Window/General/Console)
when this extension blocks the import lists every orphaned file by path.
Read that list to know exactly which files and folders need attention before
retrying.

## Disabling this check

If you have a deliberate reason to keep scripts in Assembly-CSharp and want
to skip this check, you can disable **Ensure Assembly Definitions** on the
Import Configuration settings page. Be aware that doing so removes the
protection described above, and the risks will apply.
