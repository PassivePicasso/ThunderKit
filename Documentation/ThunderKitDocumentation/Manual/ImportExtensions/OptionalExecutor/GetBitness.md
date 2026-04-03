---
{ 
	"title" : "Get Bitness",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/8840720793112784295b7c9b06af7493){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[GetBitness](documentation://GUID/087669654ec3c5445ac7bb8e79b56a3f)

Reads the PE header of the game's executable to determine whether the game is
32-bit or 64-bit. Stores the result in `ThunderKitSettings.Is64Bit`.

This extension executes at `-1,000,000` priority (`Constants.Priority.GetBitness`).

## Platform Support

Windows only. Throws `ArgumentException` on non-Windows platforms because the
PE header format is specific to Windows executables.

## How It Works

Reads the DOS header at offset `0x3C` to locate the PE signature, then reads
the machine type field from the COFF header. A value of `0x8664` (AMD64)
indicates a 64-bit executable; any other value is treated as 32-bit.
