---
{ 
	"title" : "Import Assemblies",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/a87a9f1780c348d4080afaf9971d3a7e){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[ImportAssemblies](documentation://GUID/b216ba4bf77cd2b4eacfed464cc6540e) 

Copies managed assemblies (`.dll` files) from the game's `Managed` folder into
the Unity Package Manager package at `Packages/{GameName}/`. If the game has a
`Plugins` folder under its data directory, those files are also imported into
`Packages/{GameName}/plugins/`.

This extension executes at `3,000,000` priority (`Constants.Priority.AssemblyImport`).

## Assembly Filtering

ImportAssemblies uses a blacklist/whitelist system to decide which assemblies to
copy:

- **Blacklist** -- By default, all assemblies currently loaded in the Unity
  Editor domain. This prevents importing Unity's own DLLs, editor assemblies,
  and other assemblies that are already available in the environment.

- **Whitelist** -- By default, all files already present in the package
  destination folder. This preserves assemblies from a previous import.

- **Rule** -- The whitelist overrides the blacklist. If a filename appears on
  both lists the assembly IS imported.

Both lists can be customized through sub-extensions (see below).

## Sub-Extensions

ImportAssemblies discovers and runs three types of sub-extension at editor load
time. Sub-extensions are found in assemblies marked with
`[assembly: ImportExtensions]` and are ordered by Priority descending, the same
as OptionalExecutors.

- [AssemblyProcessor](documentation://GUID/4a190cef70983534c9b13cf066c986ae)
  -- Redirect the source path of an individual assembly.
- [BlacklistProcessor](documentation://GUID/8fc5513479ffdf544bcdedc3f58edb1c)
  -- Modify the set of blacklisted filenames.
- [WhitelistProcessor](documentation://GUID/7c18e22efc965c6408abb5ab47cd04aa)
  -- Modify the set of whitelisted filenames.

## Import Process

1. Assembly reload is locked and asset editing is paused so Unity does not
   attempt to compile or import files mid-copy.
2. The blacklist and whitelist are built and passed through their respective
   processors.
3. Each `.dll` in the game's `Managed` folder is evaluated:
   - The path is passed through every AssemblyProcessor.
   - If the filename is on the whitelist, or is NOT on the blacklist, the file
     is copied to the destination and a `.meta` file is generated.
4. If a `Plugins` folder exists under the game data directory the same filtering
   and copy process runs for those files.
5. Assembly reload is unlocked and asset editing resumes.

If an individual file copy fails a warning is logged and the import continues
with the remaining assemblies.
