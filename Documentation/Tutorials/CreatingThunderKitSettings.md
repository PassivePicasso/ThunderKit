---
{ 
	"title" : "Creating new ThunderKitSetting",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---

# Overview

The [ThunderKitSetting](assetlink://GUID/87aed674429cded4f8f0b47b0c3d588c) is a ScriptableObject derived class that works via a Singleton aproach. A ThunderkitSetting can be used for storing specific data that ThunderKit can later use for it's Systems.

ThunderKitSettings can be later inspected with the Inspector, or via Thunderkit's [Settings](menulink://Tools/ThunderKit/Settings) window, where more extensible features can be shown.

To create a ThunderKitSetting, simply create a new class and inherit `ThunderkitSetting`, and add a using clause with `using ThunderKit.Core.Data`

## ThunderKitSetting functionality

### Initializing the ThunderKitSetting asset

In case you need to initialize the settings file by putting default values, you can do so by overriding the `Initialize()` method. This method will only run Once when calling GetOrCreateSettings, and ThunderKit creates the Setting file instead of retreiving it.

#### Remarks:
* You can call `GetOrCreateSettings<T>()` from anywhere to get access to your Setting's data.

### Creating the UI for your ThunderkitSetting

ThunderkitSetting uses UIToolkit for displaying the setting interface, The setting interface can use VisualTreeAssets and Class Lists. While the usage of the aformentioned two arent covered in this tutorial, The tutorial does cover how to create the SettingsUI via code.

ThunderKitSetting creates the SettingsUI for your Settings file by making it look like its being inspected thru the Inspector, while ommiting the m_Script object field, If you want to implement a more specific UI or UI behavior, override the `CreateSettingsUI(VisualElement rootElement)` method and ommit calling the base method.

The ThunderKitSetting class comes with a `CreateStandardField(string fieldPath)` method, this method takes in the name of a serializedProperty of your asset and creates a PropertyField, alongside giving it custom CSS class qualities. This can be used to quickly create fields for your Settings UI.

Once you add your fields to the rootElement, call `rootElement.Bind(new SerializedObject(this))`

#### Remarks:
* ThunderKitSetting accepts Markdown Elements.