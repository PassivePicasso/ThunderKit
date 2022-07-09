---
{ 
	"title" : "Addressable Browser",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ComposableObject_2X_Icon" ]
}

---

### About

The Addressable Browser provides a way to view a Addressable catalog assets. 
The Directories listing are groupings of assets by their address, treating 
`/` and `\`,  as directory separators. Addresses which do not contain either
of these directory separators will be placed in a group called `Assorted` and
all Scenes will be added to a group called `Scenes` in addition to their 
original group location.

Selecting an entry under the Directories listing will reveal all assets with an
address that starts with the selected entry.

You can use this browser to locate assets and get information about them with 
the Inspector window.

### Search Field

The Search field at the top of the Addressable Browser provides the ability to
find assets using simple plain text searches on addresses and types as well as
supporting regular expressions when enabled by the checkbox.

#### Type Search

You can filter on Namespaces and Types of assets by using the `t:` literal.

For example if you want to search for all GameObjects you can enter 
`t:GameObject` into the search field.

Alternatively, if you wanted to search for all asset types defined in the 
UnityEngine namespace you could enter `t:UnityEngine`

### Case Sensitivity

The search field is Case Sensitive by default, you can ignore case by selecting
`Ignore Case` in the option picker.

### Inspecting Assets

If you open the [Inspector Window](menulink://Window/General/Inspector) you can
select assets listed in the Addressable Browser to view the Main Asset.

An Addressable Object Hierarchy is planned to allow you to examine assets 
without having to create temporary instances of them in your scenes.


### Instantiating Assets

ThunderKit and the Addressable Browser do not provide the ability to 
instantiate objects listed in their views due to limitations in how Unity saves
data about assets loaded in AssetBundles.

Research into this area continues with some concepts which may allow this to be
worked around.