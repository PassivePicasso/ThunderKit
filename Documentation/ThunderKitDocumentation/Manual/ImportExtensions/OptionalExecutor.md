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

### Core

 - { .cfw200 }[CheckUnityVersion](documentation://GUID/cbec789a5a4890b41844efe9f2069e31)
[Source](assetlink://GUID/8dcb1359c6da7c049b1063e3561a1ecf) 

 - { .cfw200 }[DisableAssemblyUpdater](documentation://GUID/d8fd47d44859b0d4d9b38ca56a15980f)
[Source](assetlink://GUID/208b77589c09e314aa3dbfae9117393a) 

 - { .cfw200 }[EnsureAssemblyDefinitions](documentation://GUID/c4d83e2fa1b7609857e4f82b3d6c0f15)
[Source](assetlink://GUID/b7f32e9d1a0c584763d21af8c5b9e049) 

 - { .cfw200 }[ImportAssemblies](documentation://GUID/b216ba4bf77cd2b4eacfed464cc6540e)
[Source](assetlink://GUID/a87a9f1780c348d4080afaf9971d3a7e)

 - { .cfw200 }[ImportProjectSettings](documentation://GUID/f6ef601f07def774daf73785ec0540ea)
[Source](assetlink://GUID/3b40885578be10f4785f1fa347e9fefa) 

 - { .cfw200 }[CreateGamePackage](documentation://GUID/c72319cdfed39d34caab9a31e63e23ad)
[Source](assetlink://GUID/a4e66fd1b2f0a6b4e951af502eba5a2b) 

 - { .cfw200 }[GetBitness](documentation://GUID/087669654ec3c5445ac7bb8e79b56a3f)
[Source](assetlink://GUID/8840720793112784295b7c9b06af7493) 

 - { .cfw200 }[Beep](documentation://GUID/9b2e0ee349f56304b8d636039c4a8451)
[Source](assetlink://GUID/0cf0398e0ff60b641a1c9a78c649cbae) 

 - { .cfw200 }[PromptRestart](documentation://GUID/82266e1ea1d3dbe44bf55f96c4d240ea)
[Source](assetlink://GUID/52610fcf3c7c01e43ad95185897e1eb5) 

### Abstract Base Classes

 - { .cfw200 }[ManagedAssemblyPatcher](documentation://GUID/ce92779cb49e6bb448fd6987a24d4296)
[Source](assetlink://GUID/c0960d561d36deb4aac684c83e4f0e74) 

 - { .cfw200 }[UnityPackageInstaller](documentation://GUID/03891ed5d95f7ab48886fac5c76769b2)
[Source](assetlink://GUID/213e13d5b2469964d921c60eadde042c) 

 - { .cfw200 }[UnityPackageUninstaller](documentation://GUID/741f8e5d5c63e5640bbf7c9334a597a9)
[Source](assetlink://GUID/469f8ad306016a44e877a98c0db1d815) 

### Addressable & Thunderstore

 - { .cfw200 }[ImportAddressableCatalog](documentation://GUID/84c7a73e402727b41ac40b5245504aad)
[Source](assetlink://GUID/3bb24ae4d588a7b4fbc1757e2fb5fd78) 

 - { .cfw200 }[AddressableGraphicsImport](documentation://GUID/5e4b823bc1bc5954cb1e3fd52663a61f)
[Source](assetlink://GUID/de95f17b2fc0cb74cba1e1af10e1d4f4) 

 - { .cfw200 }[ThunderstorePackageInstaller](documentation://GUID/ca5cdbe1c4ee63646b3515f5995776b0)
[Source](assetlink://GUID/f1e1d03284e715444951e6ec9eb21e73) 


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
abstract class can be extend to make new import extensions. All 
OptionalExecutors have the ability to be enabled or disabled to modify the 
setup of a project.

OptionalExecutors enable the ability to easily reproduce setup steps in for a
project.

The following is an example of an OptionalExecutor implementation. This 
extension will display on the ThunderKit Settings Import Configuration page.
While enabled, each time the Import process is run the HelloWorldImporter will
log "Hello world" to the Unity [Console.](menulink://Window/General/Console)
   ```cs
    using ThunderKit.Core.Config;
    using UnityEngine;

    namespace HelloWorldImporter
    {
        public class HelloWorldImporter : OptionalExecutor
        {
            public override int Priority => int.MaxValue;

            public override string Description => "Logs Hello World";

            public override bool Execute()
            {
                Debug.Log("Hello world");
                return true;
            }
        }
    }
   ```
