### Wait, Read this before moving on

The Integrated ThunderKit Documentation system is a markdown based [UIToolkit](https://docs.unity3d.com/2018.4/Documentation/Manual/UIElements.html) templating system.
ThunderKit's [MarkdownElement](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Markdown/MarkdownElement.cs) is a VisualElement which renders Markdown using the 
[Markdig](assetlink://Packages/com.passivepicasso.thunderkit/Editor/ThirdParty/MarkDig/license.txt) library. 

Go to the [Markdig GiHhub Repository](https://github.com/xoofx/markdig) for more information about Markdig.

The ThunderKit Markdown implementation extends markdown links with extra functionality to provide an integrated experience.
Throughout the ThunderKit documentation you will find various types of links and each color represents specific functionality.

Hovering over a link will show you what URI the link will be executing when you click it.

- [Menu Links](menulink://) are shortcuts to menu items in the Unity main menu, these are for convenience when you may need a window for more information from documentation.

- [Asset Links](assetlink://) are links to assets in the project, clicking on these links will
ping and select the asset, this means it will be revealed in your project window, and shown in 
your inspector. Open the [Project window](menulink://Window/General/Project) and 
[Inspector window](menulink://Window/General/Inspector) to make use of these asset links.

- [Internet Hyperlinks](http://) are links to websites such as the [Unity Manual](https://docs.unity3d.com/Manual/index.html) or [ThunderKit Git Repository](https://github.com/PassivePicasso/ThunderKit) and will launch in your default Web Browser.

This page will be updated as more types of links are added.

MarkdownElement's links do not track visitation, so the color of links will not change from usage.