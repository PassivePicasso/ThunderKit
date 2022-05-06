---
{ 
	"title" : "Using the Addressable Browser",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---

ThunderKit's Addressable Browser provides the ability for users to browse and 
inspect addressable assets provided by imported games.  Using the addressables 
features of ThunderKit is a simple process.

After installing ThunderKit and any game specific [import extensions](documentation://GUID/00b9d411fd716fd4893e9cb7c7811f0c)
open the [Settings Window](menulink://Tools/ThunderKit/Settings) then follow 
these steps.

1. Configure your import settings in the Import Configuration section
  a. Make sure you have enabled at least these importers enabled
	 * Import Assemblies 
	 * Import Project Settings with Everything selected
	 * Create Game Package
	 * Import Addressable Catalog

2. Navigate to the ThunderKit Settings section
3. Click Browse under the "Locate and Load game files for project" sub-section
4. Locate the executable of the Game whose Addressables you want to browse
5. Click Import

Be patient, the import process can take a little time.  If you see a spinning
icon in the bottom right corner the process is not done.  Unity may appear to 
be idle at times so wait a reasonable amount of time before trying to continue.

Now you can simply use the menu item [Tools/ThunderKit/Addressable Browser](menulink://Tools/ThunderKit/Addressable%20Browser)
to launch the Addressable Browser.

From here you can use a simple search tool to look for the assets you want to
inspect. Open the [Inspector Window](menulink://Window/General/Inspector) to 
see more details about the inspected asset.

The Addressable Browser is under active development