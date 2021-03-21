### Wait, Read this before moving on

The ThunderKit Documentation is built with a combination of Unity's UIToolkit (UIElements) and a custom UIToolkit element, the [MarkdownElement](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Markdown/MarkdownElement.cs) built off of
[Markdig](assetlink://Packages/com.passivepicasso.thunderkit/Editor/ThirdParty/MarkDig/license.txt).

Throughout the ThunderKit documentation you will see text underlined with various colors, these are links and each color is a different type of link.
Hovering over a link will show you what URI the link will be executing when you click it.

- Red Links: These are links to assets in the ThunderKit project, clicking on these links will ping and select the asset, this means it will be revealed in your project window, and shown in your inspector.
- Green Links: These are Menu Item links. Clicking these links will invoke a menu item in Unity.
- Blue Links: These are links to websites such as the [Unity Manual](https://docs.unity3d.com/Manual/index.html) or [ThunderKit Git Repository](https://github.com/PassivePicasso/ThunderKit)

MarkdownElement's links do not track visitation, so the color of links will not change from usage.

The ThunderKit Markdown implementation extends markdown links with extra functionality to provide an improved integration experience.

While reading through ThunderKit documentation it will be helpful to have the [Project window](menulink://Window/General/Project) and [Inspector window](menulink://Window/General/Inspector) open so that you can see the additional information made available by these links.



