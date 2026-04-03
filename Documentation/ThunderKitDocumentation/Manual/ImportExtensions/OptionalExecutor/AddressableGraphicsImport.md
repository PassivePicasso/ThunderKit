---
{ 
	"title" : "Addressable Graphics Import",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---
[Source](assetlink://GUID/de95f17b2fc0cb74cba1e1af10e1d4f4){ .absolute .pat0 .par0 }
[OptionalExecutor](documentation://GUID/e80287c690b4c0742a39805bede11894) >
[AddressableGraphicsImport](documentation://GUID/5e4b823bc1bc5954cb1e3fd52663a61f)

Abstract base class for configuring shader settings used with the Addressable
Asset System. The `Execute` method is sealed.

This class is abstract and does not appear on the Import Configuration page by
itself. Create a concrete subclass to configure game-specific shader names.

This extension executes at `-5,000` priority
(`Constants.Priority.AddressableGraphicsImport`).

## Virtual Properties

* **CustomDeferredReflection** (`string`, default `null`)
  Shader name for deferred reflection.

* **CustomDeferredScreenspaceShadows** (`string`, default `null`)
  Shader name for screenspace shadows.

* **CustomDeferredShading** (`string`, default `null`)
  Shader name for deferred shading.

## How It Works

Writes the shader name strings to `AddressableGraphicsSettings`. If the
`AddressableGraphicsSettings` type is not loaded (e.g. the
ThunderKit.Addressable.Tools assembly is missing) the extension logs a message
and skips.

## Requirements

Part of the **ThunderKit.Addressable** assembly. Requires the
ThunderKit.Addressable.Tools assembly at runtime.

## Example

```cs
using ThunderKit.Addressable.Tools;

public class MyGameGraphicsImport : AddressableGraphicsImport
{
    public override int Priority => -5000;

    public override string CustomDeferredShading =>
        "Hidden/MyGame/DeferredShading";
}
```
