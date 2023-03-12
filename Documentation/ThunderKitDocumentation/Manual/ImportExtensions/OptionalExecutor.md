---
{ 
	"title" : "OptionalExecutor",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---

Base class which provides the ability to make import extensions that can be 
enabled and disabled by users and code. 

## Fields

* **enabled**
  - When true, allows the OptionalExecutor to run when the user triggers import 

## Properties

* **Description** (**virtual**)
  - A description of the OptionalExector. This is displayed in the tooltip on
    the Import Configuration Settings page.
  
* **UITemplatePath** (**virtual**)
  - Path used by `CreateProperties()` to load a UIToolkit UXML file which will be loaded and used for rendering
    on the Import Configuration Settings Page. Use this to build more complex
	UIs for advanced Import Extensions.

## Methods

* **CreateProperties** (**virtual**)
  - Loads UITemplatePath

## Inherited Properties

* **Name** (**virtual**)
  - Name of ImportExtension, Displayed on the Import Configuration page

* **Priority** (**abstract**)
  - Integer which indicates the priority at which this extension will run. Import Extensions are ordered by their priority in descending order.

## Remarks

The [OptionalExecutor](assetlink://GUID/984ac7aa6325ea24889e2b091ee9b636) is 
the base class for all ImportExtensions. it is an abstract class which you can 
extend to make new import extensions, as see in the example on the 
[Import Extensions](documentation://GUID/00b9d411fd716fd4893e9cb7c7811f0c) page.


## Related Content


{ .cfw200 .bb2 .bdblack .cml4 }**Name** **Source** **Documentation**


{ .cfw200 .bb1 .bdblack }**Import Extension**
[Source](assetlink://GUID/dca7ed776a90eea49a7bd29ccedcec51)
[Documentation](documentation://GUID/00b9d411fd716fd4893e9cb7c7811f0c)


{ .cfw200 .bb1 .bdblack }**ImportAssemblies**
[Source](assetlink://GUID/a87a9f1780c348d4080afaf9971d3a7e)
[Documentation](documentation://GUID/b216ba4bf77cd2b4eacfed464cc6540e)

{ .cfw200 .bb1 .bdblack }**ImportProjectSettings**
[Source](assetlink://GUID/3b40885578be10f4785f1fa347e9fefa) 
[Documentation](documentation://GUID/3b40885578be10f4785f1fa347e9fefa)

{ .cfw200 .bb1 .bdblack }**CreateGamePackage**
[Source](assetlink://GUID/a4e66fd1b2f0a6b4e951af502eba5a2b) 
[Documentation](documentation://GUID/c72319cdfed39d34caab9a31e63e23ad)

{ .cfw200 .bb1 .bdblack }**UnityPackageInstaller**
[Source](assetlink://GUID/213e13d5b2469964d921c60eadde042c) 
[Documentation](documentation://GUID/03891ed5d95f7ab48886fac5c76769b2)

{ .cfw200 .bb1 .bdblack }**UnityPackageUninstaller**
[Source](assetlink://GUID/469f8ad306016a44e877a98c0db1d815) 
[Documentation](documentation://GUID/741f8e5d5c63e5640bbf7c9334a597a9)

{ .cfw200 .bb1 .bdblack }**ManagedAssemblyPatcher**
[Source](assetlink://GUID/c0960d561d36deb4aac684c83e4f0e74) 
[Documentation](documentation://GUID/ce92779cb49e6bb448fd6987a24d4296)

{ .cfw200 .bb1 .bdblack }**PromptRestart**
[Source](assetlink://GUID/52610fcf3c7c01e43ad95185897e1eb5) 
[Documentation](documentation://GUID/82266e1ea1d3dbe44bf55f96c4d240ea)

{ .cfw200 .bb1 .bdblack }**Beep**
[Source](assetlink://GUID/0cf0398e0ff60b641a1c9a78c649cbae) 
[Documentation](documentation://GUID/9b2e0ee349f56304b8d636039c4a8451)

{ .cfw200 .bb1 .bdblack }**GetBitness**
[Source](assetlink://GUID/8840720793112784295b7c9b06af7493) 
[Documentation](documentation://GUID/8840720793112784295b7c9b06af7493)