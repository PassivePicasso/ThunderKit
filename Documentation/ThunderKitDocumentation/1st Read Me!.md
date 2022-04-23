---
{ 
	"title" : "About this Documentation",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Documentation_2X_Icon" ]
}

---

## Integrated Links

The ThunderKit Documentation is enhanced by multiple types of links that add Unity integration features.

This Documentation is best viewed in Unity with ThunderKit's Markdown system.

Hovering over a link will show you what URI the link will be executing when you click it.

- [Menu Links](menulink://) use the `menulink://` scheme and are shortcuts to menu items in the Unity main menu, these are for convenience when you may need a window for more information from documentation.

- [Asset Links](assetlink://) use the `assetlink://` scheme and are links to assets in the project, clicking on these links will
ping and select the asset, this means it will be revealed in your project window, and shown in 
your inspector. Open the [Project window](menulink://Window/General/Project) and 
[Inspector window](menulink://Window/General/Inspector) to make use of these asset links.

- [Internet Hyperlinks](http://) are links to websites such as the [Unity Manual](https://docs.unity3d.com/Manual/index.html) or [ThunderKit Git Repository](https://github.com/PassivePicasso/ThunderKit) and will launch in your default Web Browser.

- [Documentation Links](documentation://) use the `documentation://` scheme and are shortcuts to Documentation pages. These can be used to quickly browse between relevant topics, such as a manifest datum and it's respective pipeline job.

This page will be updated as more types of links are added.

MarkdownElement's links do not track visitation, so the color of links will not change from usage.

## Copying Text

The documentation in its current form doesn't easily allow free selection of text.
```
However, you can select and copy from code blocks like this one
```

## About ThunderKit Documentation

ThunderKit Documentation is built on a [UIToolkit](https://docs.unity3d.com/2018.4/Documentation/Manual/UIElements.html) Markdown system.

See the [Documentation Folder](assetlink://GUID/8a4cd14903a156d48ac381bd86e23e48) for the Markdown files that makes up this documentation.

ThunderKit's [MarkdownElement](assetlink://GUID/ec19b76b765719a4fb4383a4fa9324ea) renders Markdown using  
[Markdig 18.3](assetlink://GUID/a3cea14f6fefce94082492a3e8df5358) 

Go to the [Markdig GitHub Repository](https://github.com/xoofx/markdig) for more information about Markdig.
