---
{ 
	"title" : "OptionalExecutor",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/a87a9f1780c348d4080afaf9971d3a7e){ .absolute .pat0 .par0 }
[Import Extensions](documentation://GUID/00b9d411fd716fd4893e9cb7c7811f0c) >
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894)

## Description

Base class which provides the ability to make import extensions that can be 
enabled and disabled by users and code. 



## Derived 
 - { .cfw200 }[ImportAssemblies](documentation://GUID/b216ba4bf77cd2b4eacfed464cc6540e)
[Source](assetlink://GUID/a87a9f1780c348d4080afaf9971d3a7e)

 - { .cfw200 }[ImportProjectSettings](documentation://GUID/f6ef601f07def774daf73785ec0540ea)
[Source](assetlink://GUID/3b40885578be10f4785f1fa347e9fefa) 

 - { .cfw200 }[CreateGamePackage](documentation://GUID/c72319cdfed39d34caab9a31e63e23ad)
[Source](assetlink://GUID/a4e66fd1b2f0a6b4e951af502eba5a2b) 

 - { .cfw200 }[UnityPackageInstaller](documentation://GUID/03891ed5d95f7ab48886fac5c76769b2)
[Source](assetlink://GUID/213e13d5b2469964d921c60eadde042c) 

 - { .cfw200 }[UnityPackageUninstaller](documentation://GUID/741f8e5d5c63e5640bbf7c9334a597a9)
[Source](assetlink://GUID/469f8ad306016a44e877a98c0db1d815) 

 - { .cfw200 }[ManagedAssemblyPatcher](documentation://GUID/ce92779cb49e6bb448fd6987a24d4296)
[Source](assetlink://GUID/c0960d561d36deb4aac684c83e4f0e74) 

 - { .cfw200 }[PromptRestart](documentation://GUID/82266e1ea1d3dbe44bf55f96c4d240ea)
[Source](assetlink://GUID/52610fcf3c7c01e43ad95185897e1eb5) 

 - { .cfw200 }[Beep](documentation://GUID/9b2e0ee349f56304b8d636039c4a8451)
[Source](assetlink://GUID/0cf0398e0ff60b641a1c9a78c649cbae) 

 - { .cfw200 }[GetBitness](documentation://GUID/087669654ec3c5445ac7bb8e79b56a3f)
[Source](assetlink://GUID/8840720793112784295b7c9b06af7493) 


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

The OptionalExecutor is the base class for all ImportExtensions. It is an 
abstract class which you can extend to make new import extensions, as see in
the example on the 
[Import Extensions](documentation://GUID/00b9d411fd716fd4893e9cb7c7811f0c) 
page.
