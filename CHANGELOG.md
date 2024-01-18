## 8.0.5

### Improvements and Fixes

* Update AssetsTools to V3
* Fixed ThunderKit Addressable Browser having compressed item height in some
  environments
* Fix Assembly-CSharp and related files being deleted by Unity in some versions
  Note: this was the cause of the infamous issue where scripts would be 
  unavailable.  If you continue to encounter this problem let us know on discord.
* Fix Scene files not showing up in Addressable browser

## 8.0.4

### Improvements and Fixes

* Added setting to enable processing CSProj files for local UPM Packages to fix
  missing folder structure information
* Added setting to generate a CSProj for ThunderKit's non-code assets. This is
  primarily for maintainers.
* Added documentation for BatchMode
* Improve import extension documentation
* Fix Packages not indicating their install state correctly
* Fix Scripting Symbols not being reliably generated for ThunderKit installed
  packages
* Fixed missing PreviewStage Scene file for older versions of Unity
* Fixed an issue where Control Characters could get added to PathReference 
  fields on Linux. Thanks to @aldelaro5 for identifying this issue.

### Community Contributions

* Fix issue with ThunderKit.Common.Constants colliding sometimes.

* Fix issue with DLL to CS reference migration utility that would prevent some
  assets from correctly being converted due to not accounting for negative 
  FileIDs.
  Thanks to @KevinfromHP for this fix.

## 8.0.3

### Fixes

* Fix AssetsTools.Net.MonoReflection AssemblyDefinition not being limited to
  the editor platform

## 8.0.2

### Fixes

* Fix AssetsExporter AssemblyDefinition not being limited to the Editor 
  platform
* Fix ImportExtension Loading not working older Unity versions (<2019.2)
  This was due to TypeCache not being available in older unity versions and 
  was missed during the testing process
* Fix QuickAccess Pipelines/Manifests throwing exceptions when there are no
  QuickAccess Pipelines/Manifests
* Fix PipelineLogWindow throwing exceptions when no PipelineLog has been 
  assigned to it
* Fix not correctly attributing community changes in prior update

## 8.0.1

### Fixes

* Fix AsmDef platform assignment interfering in build processes

### Community Contributions

* Fix Assembly-CSharp-firstpass.dll and other Assembly-CSharp*.dll files not
  being managed by ThunderKit.
  Thanks to @EldritchCarMaker for this fix.

## 8.0.0

### New Features

* Add Addressable Prefab Preview stages for all Unity Versions.
  This feature allows you to inspect Prefab objects in a SceneView style
  environment. Allowing you to look at the Transform Hierarchy of prefabs, and
  inspect their configuration. In Unity 2018 this is a custom
  SceneView, while newer Unity versions make use of the PreviewSceneStage
* Add Limited Support for SpaceDock as a PackageSource.  This will allow KSP mod 
  developers to utilize SpaceDock to download mod packages from for supporting 
  their projects, however discovering and resolving dependencies is currently 
  not supported
* Add Log Level Filtering to Pipeline Log window
* Add Loading indicators to loading PackageSources in the Package Manager
* Add Log Cache details and cache clear button to ThunderKitSettings
* Add Markdown Image Cache details and cache clear button to ThunderKitSettings
* Add Disable Assembly Updater ImportExtension to encourage disabling it
* Add Unity Version Check ImportExtension to assist in troubleshooting and 
  complex environment configurations. **This should generally not be disabled.
  Disabling this to specifically use a verison of Unity that does not match an
  imported game is unsupported**

### Improvements

* Improve the way Packages are listed in the Package Manager
* Add PackageManager resizing between the package list and view
* Improve load times and consistency of PackageSources
* Create LoadingSpinner, a reusable UIToolkit loading indicator
* Improve package selection highlighting for Dark and Light themes
* Improve Package Loading to incrementally populate as results are provided
* Improve PackageSource re-usability by abstracting auto-loading
* Improve PackageView to utilize MarkdownElements for the Package Header, Body, 
  and Footer sections so that their content is more customizable per 
  PackageSource. Additional work needs to be done here to improve the workflow 
  and modularity of this feature
* Improve the Browse button to open in the selected path if it is not empty
* Improve image caching solution in MarkdownElement system
* Improve Pipeline Log View to with log level filtering
* Update Copy PipelineJob documentation
* Improve logging and code design of Copy
* Add OptionalExecutor documentation
* Improve some documentation navigation
* Improve addressable integration to avoid interfering with normal addressable
  package usage
* Change PromptRestart to an OptionalExecutor for consistency
* Change Beep on import completion to an OptionalExecutor for consistency
* Generally improve code quality

### Fixes

* Fix ImportExtensions not updating correctly when extensions are added or 
  removed
* Fix ImportExtensions erasing serialized data in some circumstances
* Fix issue where import could fail for certain build layouts
* Fix an issue in the latest version of Unity 2022.2 that would cause package
  installs to fail
* Fix MarkdownElement not loading all images correctly or consistently due to
  over aggressive cancelling and object clean up
* Fix a memory leak in the Markdown system tied to not releasing displayed 
  images

### Community Contributions

Thanks to @KingEnderbine for these improvements

#### Features

* Add ability to include unity meta files to the Files PipelineJob and Manifest
  This improves support for source re-distribution and asset redistribution via
  packages

#### Improvements

* Improve ImportConfiguration system to reduce the occurrence of unnecessary 
  work

#### Fixes 

* Fix ComposableObject not properly removing elements in some versions of Unity
  This fixes a pesky issue that affected some users where it would cause 
  Missing Script errors in ComposableObjects such as PipelineJobs and Manifests
* Fix ThunderKitSettings sometimes overwriting itself during domain 
  initialization 
* Fix ImportConfiguration not saving the last ConfigurationIndex used leading
  to the ImportSystem repeating final steps
* Fix Quick Access selections not saving correctly
* Fix Quick Access Toolbar not initializing correctly


## 7.1.2

### Fixes

* Fix StageAssemblies failing builds in some cases for non-Unity 2020 editors

## 7.1.1

### Fixes

* Fix StageAssemblies using facilities not available in Unity 2019

## 7.1.0

### Improvements

* Improve StageAssemblies error reporting to provide build errors with a 
  context for each build error in the log entry
* Improve formatting of StageAssembly error reports to include source file
  links for errors

### Fixes

* Fix Assemblies not building correctly on Unity 2020.x

## 7.0.0

Big thanks to @Nebby1999 for this building the majority of this update

### Possible Breaking Change

* Fix ScriptingSymbolManager using `-` instead of `_` for spaces in symbols due
  to `-` not being supported by Visual Studio or .NET in defined symbols.

  This is only a breaking change if your project has used Define Constraints 
  in Assembly Definitions, otherwise this should have no impact.

### Improvements

* Added PackageSource.InstallPackages for ImportExtensions that need to install
  multiple packages.  This allows for a significant improvement in install time
  for these types of extensions.
  Improvements to the ThunderKit Package Manager related to this have not been 
  made but are planned.

* Add Post import prompt requesting the user restarts unity. A restart avoids a
  class of erronous experiences where the Unity environment has not loaded
  entirely after an import.

* Add a Beep when the importing process has completed (Default ON)

* Add a toggle to disable post import beep in ThunderKit Settings.

### Fixes

* Fix ImportConfiguration not updating correctly when new ImportExtensions are 
  loaded into the environment.

## 6.1.1

* Fix Package.json version

## 6.1.0

* Update ExecuteProcess to be a FlowPipelineJob instead of a PipelineJob 
  allowing it to utilize manifest data

## 6.0.2

* Fix exception when scanning for types in the Add Component menu preventing it 
  from opening (@Nebby1999)

## 6.0.1

### Fixes

* Fix ThunderKit's package.json to reflect the current version

## 6.0.0

### BREAKING CHANGES

This update includes breaking changes to how GUIDs are generated for assemblies.
Before updating to ThunderKit 6.0.0 you should be sure that your project's 
assemblies are using the Stabilized Assembly GUID mode.

If you're not using the Stabilized Assembly GUID mode, or if you're unsure if 
you are, open up [ThunderKit Settings](menulink://Tools/ThunderKit/Settings) 
and navigate to Import Configuration.
In the ImportConfiguration screen, locate the Import Assemblies entry and then
select "Stabilized" from the dropdown menu and finally press Update.
This will convert any assemblies managed by ThunderKit to use the Stabilized
GUID mode and should limit the impact of updating to ThunderKit 6.0.0

### Improvements

* Utilize blacklist and whitelist AssemblyProcessors for processing plugins in
  addition to managed assemblies during import

* Remove Original and <2.0.0 AssetRipper GUID generation modes 
  This can be a breaking change for some projects
  (Thanks to @Nebby1999 for this contribution)

* Modify ImportConfiguration to load ImportExtensions more defensively
* Improve PathReference to minimize calls to AssetDatabase.FindAssets()
  This significantly improves CPU and RAM costs durting pipeline execution
  (Thanks to @KingEnderBrine for this contribution)

* Rework the Copy PipelineJob to allow non-overwriting copies, this chagne is
  setup to be backwards compatible.
  (Thanks to @KYPremco for this contribution)

### Fixes

* Fix installing latest package version from a PackageSource causing 
  invalid Version information to propogate in a project
  (Thanks to @Nebby1999 for this contribution)

* Clean up ChangeLog entries with widths over 80 characters

## 5.4.5

### Fixes 

* Fix issue where loading scenes from the AddressableBrowser would fail even 
  when in play mode

## 5.4.4

### Fixes

* Fix issue where the Import button would launch the LocateGame function and 
  require the user to import the game to close the dialog.

## 5.4.3

### Fixes

* Fix PackageSources not rendering correcly in Packages window or 
  PackageSourceSettings

## 5.4.2

### Fixes

* Fix issue in PackageHelper that would cause ThunderKit to try to call a 
  method which doesn't exist in Unity 2020.3

## 5.4.1

### Fixes

* Fix version number in package.json
* Fix Casing of USS and Documentation/Tutorials folder for Linux
* Fix version compatibility issue with 2019.2.17
* Fix ImportAssemblies not respecting the fact that Linux can use Windows
  binaries
  - This is handled by just not using an extension filter in the executable 
    file search.

* Fix ImportAssemblies not importing plugins folder contenst on Linux
  - This is also because of an issue with extensions and is also handled by
    just removing filtering as there is no known reason to filter the import 
    of the plugins folder at this time.
* Fix ImportConfiguration creating its sub assets twice during install of 
  ThunderKit in some Unity versions
* Fix a warning for PipelineToolbar about non-awaited call to an async task 
  returning method
* Fix warning when trying to search for ThunderKitSettings when no 
  ThunderKitSettings have been created yet.


## 5.4.0

### Features

#### BatchMode (Command line execution)
Added the ability to execute ThunderKit Pipelines using the Unity BatchMode and 
Command line interface
To use this feature you will need to execute the appropriate version of Unity
and supply atleast the `-projectPath`, `-executeMethod` and `-pipeline` 
arguments, and optionally the `-batchmode` and `-manifest` arguments.
Using these command line arguments you can execute ThunderKit Pipelines.

To learn more about Unity's command line arguments refer to 
[Unity Editor command line arguments](https://docs.unity3d.com/Manual/EditorCommandLineArguments.html)

Arguments:
* `-pipeline=path` - Project Relative Path to target Pipeline 
* `-manifest=path` - Project Relative Path to target Manifest
* `-show-log-window` - Enables displaying the Pipeline Log window during 
  execution

To execute the pipeline you must call the execution method. You can do this 
using the Unity Command line argument -executeMethod and providing 
`ThunderKit.Core.Pipelines.Pipeline.BatchModeExecutePipeline` next.

The following demonstrates a complete setup
```
C:\Program Files\Unity\Hub\Editor\2019.4.26f1\Editor\Unity.exe" -batchmode 
-projectpath F:\Projects\Unity\ExampleProject
-manifest="Assets/ThunderKitAssets/BundleManifest.asset"
-pipeline="Assets/ThunderKitAssets/BuildBundles.asset"
-executeMethod ThunderKit.Core.Pipelines.Pipeline.BatchModeExecutePipeline  
```

#### Other
* Added FindFile PathComponent

### Improvements
* Updated BepInExPackSource to be more flexible
* Fixed most cases in the change log where it didn't adhere to the 80 character
  limit rule suggested for Markdown.

### Fixes
* Fixed case where winhttp.dll PathReference could create a directory instead
  of copying the file when the winhttp.dll source is not found
* Fixed an compatibility issue in StageAssetBundles for Unity 2019.1 and 2019.2

## 5.3.1

### Improvements and Features
#### General
* Change the default mode for opening Markdown files to Unity External Editor

#### Stage Assemblies
* Improved StageAssemblies to execute its builds sequentially to ensure all 
  assemblies build as expected
* Improved UNet Weaver support for more versions and implementations

#### Addressable Support
Improve loading configuration to avoid cases where Addressables becomes 
unloaded

#### Addressable Browser
* Add Type element to Asset rows to show what the type of asset is
* Add Provider element to Asset rows to show what provider is used to load the
  asset
* Add documentation for Addressable Browser
* Add Help button to Addressable Browser that displays the added documentation
* Migrate from indexing Addressable Addresses as strings to using the 
  IResourceLocations directly

#### PipelineLog Window
  * The PipelineLogWindow had inconsistent display and function of the details 
    button and has been fixed.
  * The LogContextWindow had a sidebar listing the contexts, this was replaced
    with a popup control to minimize whitespace

### Fixes
* Fix a problem with Quick Access Toolbar where elements couldn't be removed as
  expected
* Fix Stage Dependencies being disabled by default
* Remove testing code from StageAssemblies that shouldn't have been deployed
* Fix an issue where removing an element from a ComposableObject (Pipeline, 
  Manifest, PathReferences) would remove 2 instead

## 5.2.3

* Fix ThunderstoreSource only loading for the first source community

## 5.2.2

* Make AddressableGraphicsSettings.AddressablesInitialized event public for 
  consumption
* Add ManagedAssemblyPatcher base class for building binary assembly patching
  importers
* Fix issues with PackageSource initialization, cataloging and loading

## 5.2.1

* fix issue where the ThunderKit Extensions PackageSource would be re-created 
  multiple times
  
Users may need to delete their existin PackageSources and 
PackageSourceSetttings to fix data issues caused by this bug

## 5.2.0

### Addressable Browser Type Filtering

Implemented Type filtering in Addressable Browser search with a similar 
implementation to the Project Window.

Users can now enter t:TypeName to filter the addressable list by type. This 
check does a case sensistive contains check on the type full name.  This allows
users to filter not only by type but partial type names and namespaces.

For example, you can filter for all Prefabs by using the filter `t:GameObject`
Another example is that you could filter for all native Unity types by using
`t:UnityEngine`

The type filtering can be used in addition to the normal text match filtering.

The text filtering can now also use Regular Expressions for more complex 
filtering cases.

### Fixes

* Fix Null Reference Exception caused by Package source refreshing
* Fix Unity crash caused by heavy repeated calls to AssetDatabase.FindAssets
* Fix initialization error in Addressable Browser due to an incorrect type 
  lookup
* Fix some cases where addressables becomes unloaded and does not reload.
  Some cases for this issue still exist and some cases are game specific and
  may require custom import extensions to resolve.
* Fix an issue where the Import process would loop indefinitely when the 
  version of the game's Unity engine and the current Unity Editor version do
  not match.

### Community Contributions

* Fix icon assignment for ThunderKit assets created using the ScriptableHelper
  not assigning the assets icon correctly.
  - Thanks to pikokr for this contribution.

## 5.1.1

* Fix/Update change log
* Fix package version number

## 5.1.0

* Add Regular Expressions support to Addressable Browser
* Improve some styling
* Add documentation for Import Extensions
* Add tutorial for using the Addressable Browser

## 5.0.6 - 5.0.8

* Fix StageAssetBundles causing addressables to become unloaded
* Add condition to license

## 5.0.5

Fix ThunderstorePackageInstaller getting stuck in a loop when its target 
package is already installed

## 5.0.4

Note: this is technically a breaking change, but I'm not going to update the 
project to version 6.0.0, because I'm evil.

Update OptionalExectors.Execute to return a bool indicating whether or not the 
executor has completed.
This was necessary to ensure stable imports so Executors can spread their 
actions out over multiple frames as well as stall until the environment state 
is ready.

## 5.0.3

Fix missing semi-colon in MarkdownContextualMenuManipulator for Unity 2018

## 5.0.2

Fix AddressableGraphicsImport failing to load the AddressableGraphicsSettings 
due to a timing issue with compilation.

Solution ensures that compilation and updating is finished before executing the
next step in the import process.
Additionally setting the TK_ADDRESSABLE scripting define is not conducted 
synchronously

## 5.0.1
* Fix compatibility issue with Unity 2019+

## 5.0.0

### A Note about Installing ThunderKit

ThunderKit installation directions have generally had users install ThunderKit
using it's master branch, which is provided with the url 
`https://github.com/PassivePicasso/ThunderKit.git`

This was a poor precedent to set and can lead to unexpected upgrades for users.
Going forward it is recommended to install ThunderKit using a Tagged release.

For Example installing ThunderKit 5.0.0 can be done using the following url:
`https://github.com/PassivePicasso/ThunderKit.git#5.0.0`

When a project needs to be migrated to a different version of ThunderKit the 
ProjectRoot/Packages/manifest.json can be edited to change the value of the url
used to install ThunderKit.  After returning to Unity the Unity Package Manager
will detect the change and update the ThunderKit installation to the correct 
version.

### Import Rework and ImportExtensions

The process for importing games has been completely reworked into a modular
extensible system.

Now every default import step can be enabled or disabled allowing for the
implementation of custom import processes and importers designed for different
environments.

ThunderKit now loads and creates concrete types derived from ImportExtension
out of assemblies decorated with the ImportExtensionsAttribute and adds them
to the new ImportConfiguration ThunderKitSetting.

Using these features customized import steps can be developed for games,
minimizing the time starting development on new projects.

### Assembly Import Changes

The Assembly Import process has been improved with the ability to use multiple
different identity algorithms when producing assembly meta files for unity.
These identities are how Unity identifies the assembly to load MonoBehavior
and ScriptableObject Types from.

There are currently three algorithms available for producings the identities:
1. Original - This is the algorithm ThunderKit has used to since it was 
   released.
2. Stabilized - This is the same as Original, except it ensures that UTF8 
   encoding is used to generate and write the GUID.
3. AssetRipper Compatible = This is the algorithm used by AssetRippers new
   Assembly Export mode and provides the ability to interoperate with 
   AssetRipper rips and ThunderKit projects. This would allow you to copy
   a prefab from a rip into a mod project for example and have all the
   scripts be correctly loaded.

### Pipeline Quick Access Toolbar

A Pipeline Toolbar has been added to the Unity Main toolbar allowing you to
quickly select a pipeline and manifest to execute without having to search
your Project.

To add Pipelines and Manifests to your toolbar as options, select the Pipeline
or Manifest in your project and check the "Quick Access" checkbox in the 
header. This will allow you to click the appropriate selector in the toolbar
and run it using the Execute button, or view the most recent log using the Log
button.

### Addressable Support 

#### Importing

Support Addressables has been added to ThunderKit. With the new 
ImportAddressableCatalog ImportExtension you can import the catalog from a game
using addressables, enabling you to load addressaable assets in the editor.

#### Edit and Runtime usage 

This allows developers to setup simple tools to apply materials or instantiate 
prefabs
in the editor for viewing. Scripts created to do this can be setup to work both
in the editor or at runtime. See the 
[Simply Address](https://github.com/PassivePicasso/SimplyAddress)
repository for an implementation of such features.

#### Addressable Browser

The [Addressable Browser](menulink://Tools/ThunderKit/AddressableBrowser) 
provides an interface to search and explore a games Addressable Catalog.
Using this browser you can locate the assets and their addresses so that you
can use a games assets in the editor and at runtime.

The Addressable Browser allows you to search for assets based upon their 
address, name, and type.  Additional filters may come in the future.

#### Limitations

Editing Addressable Assets is limited to a code based approach and tooling
to resolve this is outside of the scope of ThunderKit. Look projects like
[Simply Address](https://github.com/PassivePicasso/SimplyAddress) that provide
generic tools for working with addressables in a modding context.

Addressable assets can't be referenced like traditional Unity assets. This
limits using addressable assets to a code based approach. However, generic 
tools
like SimplyAddress are being and can be developed to improve the ease of use.

### Documentation 

[Documentation](menulink://Tools/ThunderKit/Documentation) has been updated
to be extensible via the DocumentationRoot ScriptableObject. Creating a 
DocumentationRoot can be done via the Project Window Context menu under the
ThunderKit sub menu. A DocumentationRoot establishes a root documentation
section in the ThunderKit documentation window with the same name. 

Additionally, DocumentationRoot contains the Main Page member, which can 
be set to any Markdown file regardless of its location as the page displayed
when the DocumentationRoot is selected.

Documentation is now easier to write and extend, no longer requiring the 
creation of UXML and USS files to setup simple documentation collections.
See the
[ThunderKitDocumentation](assetlink://GUID/33d96cac9b15cba468162cf9d18ec0f3)
for an example of a working documentation collection.

Documentation has been added and the layout of the existing documentation 
has been re-organized.

Tutorials is a new and growing collection of documents that will walk you 
through getting started with usaging and extending ThunderKit. Submission 
of markdown files to grow this collection of tutorials is very welcome and 
can be submitted on discord or through a pull request on github.

### Binary Patcher BsDiff

The Binary Patcher BsDiff has been added to ThunderKit to enable import
workflows
that require modifications to game binaries for scenarios that need them such
as editor compatibility.

The ApplyPatch and CreatePatch PipelineJobs have been created to enable 
Pipeline
workflows that can utilize binary patching.

### AssetsTools.NET

A modified version of AssetsTools.NET has been included to enable importing
of project settings from games.  This is provided via the ImportProjectSettings
ImportExtension. Configure it on the Import Configuration 
[ThunderKit Settings](menulink://Tools/ThunderKit/Settings) page.

Additionally, game version checking has also been enhanced using
AssetsTools.NET

### Community Contributions

#### Edit Source / Select Source

[Documentation](menulink://Tools/ThunderKit/Documentation) pages now have a 
context menu allowing the users to select the source markdown files or edit 
them in their preferred editor, configurable from 
[ThunderKit Settings](menulink://Tools/ThunderKit/Settings)

#### Guid Stabilized links for Markdown

AssetLink and Documentation schemes for ThunderKit Markdown have been enhanced 
to allow referencing assets using their GUID. This feature enables 
documentation to remain stable even when moving files around inside the unity 
environment, or externally when the user takes care to move their associated 
meta file with them.

All Documenation has been updated to use the new GUID format for referencing
project assets, this will help ensure that documentation links remain stable
reducing maintenance cost in the future.

#### Cross Documentation Page Links

Documentation has been updated with links between documentation pages to 
improve ease of use.

 * Thanks to nebby1999 for these features

#### FlowPipeline Whitelist / Blacklist

FlowPipelines have been updated with Whitelist / Blacklist system with 
automatic updating for old asset files. This affects the Copy, Zip, and Delete 
PipelineJobs which have the ability to exclude Manifests from their run. Now 
alternatively a whitelist can be provided which causes the job to only run on 
manifests in that list.

Users should not need to update their pipelines, but upgrading to ThunderKit 5 
will make changes that are not backwards compatible to your pipeline assets.

* Thanks to KingEnderbrine for this feature

### Fixes and improvements

#### General
* Fix error in FileUtil which produced incorrect FileIds for Script references
  to Assemblies
* Make minor improvements for enhancing cross platform editor compatibility
* Stop persisting PackageGroups and PackageVersions to disk to avoid runaway 
  disk usage
* Fix a number of small memory leaks
* Add doc comments to code base (plenty more work to be done here)
* Create UXML and USS folders in ThunderKit root and move all UXML and USS too 
  them some template specific USS remains in the UXML folders next to their 
  related UXML files
* Updated ThirdPartyNotices.md

#### Markdown

* Added JsonFrontMatter for Page Headers 
  - Json FrontMatter can be used to collect a limited set of values from a
    Markdown file. See the 
    [FrontMatter Struct](assetlink://GUID/70db1552b66c4e34d88f6b33d3e0ead7) for
    details.
* Generic Attributes - Apply USS/CSS Classes to Blocks of text in markdown
* Improvements to the markdown style have been made to bring it more in line 
  with common markdown styling
* MarkdownElement now detects changes to source files and updates automatically

#### Import
* Fixed import process sometimes not completing requiring the user to change 
  focus away from and back to the editor to complete.

#### PipelineLogs
* Add setting to enable the PipelineLog window to show automatically when
  a pipeline is executed.
* Automatically update PipelineLog window when visible and a pipeline is
  executed
* Updated PipelineLog window to show a button for entries with Log Context

#### ComposableObjects
* Fix an issue with the ComposableObjectEditor which caused it to be more 
  computationally intensive than necessary
* Enance ComposableObjectEditor to allow it to render elements with Missing 
  Scripts to inform the user
  * This currently doesn't allow elements with missing scripts to be deleted
* Improved the ComposableElement Edit Script context menu item to be more 
  reliable


## 4.1.1

### Pipelines and Logging

This update introduces a system to maintain logs of Pipeline runs. These logs
saved under the Assets/ThunderKitAssets/Logs folder and grouped by Pipeline.
Pipeline Logs are rich data sets which provides listings of runtime function
and reporting of build artifacts. Logs will show you what was done during a
pipeline run, what files were copied, deleted, and created during that run.

The pipeline logs will additionally show any errors, and provide any 
potentially relevant data from the errors that could lead to resolution. These 
errors are enhanced by ThunderKit's markdown system, allowing you to click on 
source code lines to open up your code editor to the source of errors for 
further debugging. This should help developers who extend ThunderKit with 
custom PipelineJobs and PathComponents.

The most recent log for a pipeline can be launched by inspecting the Pipeline 
then clicking on the Show Log button.

The execute button for pipelines have been moved from under the 
"Add Pipeline Button" to the top left of the Pipeline Inspector. This should 
reduce incidents of accidentally firing off the Pipeline.

### Markdown Level Up

Text alignment and kerning has been improved significantly.  I'm sorry for any 
mental anguish users have suffered.

The Markdown implementation performance and output quality has been 
significantly improved. Previously the UIElementsRenderer would break all 
string literals on whitespace separation and then render each word as an 
individual VisualElement of type Label. This provided an easy way to achieve 
flow document layouting, however resulted in large documents taking an 
exceptionally long time to render.

In this update the UIElementsRenderer will now scan each ParagraphBlock 
returned by MarkDig and if the Paragraph contains only simple string literals 
will opt to render the entire paragraph in a single Label.  This reduces the 
number of elements generated in large documents by thousands. This results in 
significantly improved render times in large documents as well as faster 
layouting.

Additionally, the Markdown system now supports adding custom Schemes for 
Markdown links from external libraries which has enabled new features in 
ThunderKit.

Finally the code design of the MarkdownElement and its utilization has been 
improved to prevent cases where Markdown doesn't have the necessary visual 
styles to render correctly.

### Documentation Improvements

The Markdown improvements has allowed the introduction of Documentation page 
links to be created. Now MarkdownElements can link to specific documentation 
pages.  This hasn't been applied to all documentation to create a highly 
connected document graph yet, but additional enhancements to documentation will
be done over time.

Some documents have been reformatted to improve their layout flexibility

### Fixes and Improvements

* Automatically generate a PackageSource for the ThunderKit Extensions 
  Thunderstore

* Remove ThunderKit.Core.Editor namespace due to code clarity issues a 
  namespace named Editor creates in Unity

* Fix bugs with Pipeline flow related to Asynchronous migration

* Fix a number of cases where Exception context could be hidden

* Add a new toggle to Copy jobs that indicates if the Job should try to create 
  the target directory, default value is true

* Fixed some cases where Pipelines would run to the end instead of halting when
  encountering what should have been a fatal exception

* StageAssetBundles and StageAssemblies logging and code flow has been improved 
  to clarify common cases where these jobs will fail to execute correct

* Added and improved logging to Copy, Delete, ExecutePipepline, ExecuteProcess, 
  StageAssemblies, StageAssetBundles, StageDependencies, StageManifestFiles, 
  Zip and StageThunderstoreManifest

* Fix issue where SteamBepInExLaunch could fail to start due to formatting of
  command line args

* Fix issue in Zip that could cause the job to fail in a case it shouldn't


## 4.0.0

### Important

This update is breaking support for .NET 3.5 due to the difficulty in providing 
functional tools for certain aspects of Unity which are asynchronous. For 
people who need .NET 3.5 support, install ThunderKit using the 
`net35compatibility` branch which will receive fixes able to be ported upon 
request

```
"com.passivepicasso.thunderkit":"https://github.com/PassivePicasso/ThunderKit.git#net35compatibility",
``` 

This update changes how Manifest assets in the Unity AssetDatabase are managed. 
You will be asked to run an upgrade process that will update all your Manifests 
to the new configuration automatically. Please make sure you back up your 
projects before updating in case of any problems.

Some games do not have their Unity game version properly identified in their 
executable. Due to this, ThunderKit will now read the games globalgamemanager 
file to identify the correct Unity version for the game.  Some users may find 
they need to switch unity versions because of this change, but it is a 
necessary step to take to avoid unforseen issues.

### Known Issues

* Unity 2021.2.0b7 does not detect package installation or uninstallation 
automatically requiring the user to manually refresh the Project. This is an 
issue which appears to be a bug with Unity's AssetDatabase.Refresh call and a 
bug report will be generated for Unity Technologies to investigate. This bug 
may be resolved in newer versions of the Unity 2021.2 beta, however there are 
no games available to test against which wouldn't introduce factors that could 
muddle results. If Unity doesn't appear to import packages installed from 
Thunderstore, or doesn't appear to fully remove an uninstalled package, refresh
your project using the context menu option in the Project window, or on windows 
press Ctrl+R

* Unity 2021.2.0b7 locks up when importing and loading assemblies from packages
  or games.
  - Work-around: Kill the Unity process after it seems like the import process 
    has stopped loading new assemblies and restart Unity

### Improvements

* Unity 2021.2 beta can now succesfully install packages, however the user must
  manually refresh the project (Ctrl+R) to complete the installation.

* Pipelines and PipelineJobs now execute asynchronously to support operations 
  which require that Unity take control of processing.

* StageAssemblies previously relied on simply copying assemblies from the 
  project's Library/ScriptAssemblies folder. While fast and convenient this
  prevented users from taking control of build parameters which may be 
  necessary for their projects. StageAssemblies now allows you to specify Build
  Targets and Build Target Groups in addition to allowing you to stage debug
  databases. Due to this change StageAssemblies now builds player assemblies,
  allowing the utilization of available optimization steps the compilation
  engine provides.

* Package Installation has been improved to no longer utilize the AssetDatabase 
  to create and place files to avoid edge cases with some versions of Unity
  that prevent installation. Due to this change Manifest's now have Asset GUIDs 
  assigned by ThunderKit. This change ensures that Manifest's will easily and 
  automatically reference their dependencies, and references to dependencies 
  will continue to work after reinstallation them. This change is not backwards 
  compatible

* Added compiler directives and code to support Unity 2021.2

* Add a utility to assist in migrating Non-ThunderKit modding projects to 
  ThunderKit by updating asset references from Dll references to Script file
  references. This is available under Tools/ThunderKit/Migration 

* Added error messaging for PathReferences and Manifests

### Fixes and Changes

* Fix cases where Progress bars may not update or close as expected
* Fix ManifestIdentities not always being saved during package installation
* Fix issue where somtimes PackageSourceSettings will empty its reference array
  requiring manual repopulation
* Fix PackageManager not removing Scripting Defines when removing a ThunderKit
  managed Package

## 3.4.1

### Fixes

* Fix an issue where scanning some assemblies would result in a 
  ReflectionTypeLoadException preventing the settings window from loading.

## 3.4.0

### PackageSource Management

PackageSources have been updated with central management and the ability to 
setup multiple sources of the same types.

You can now manage your Package Sources in the 
[ThunderKit Settings Window](menulink://Tools/ThunderKit/Settings).
In the ThunderKit Settings window you will be able to Add, Remove, Configure,
Rename and Refresh your PackageSources.

### ThunderKit Extensions Thunderstore

With the ability to add multiple PackageSources to your project, you can now 
add the ThunderKit Extensions Thunderstore as a source. Like all Thunderstores,
this provides the ability for the community to add resources that help grow the
platform in a modular way.

If you would like to take advantage of the ThunderKit Extensions Thunderstore,
Add a new ThunderstoreSource to your PackageSources and set the url to
``` https://thunderkit.thunderstore.io ```


## 3.3.0

### Steam Launch support

Some Unity games require that Steam launches them in order to successfully 
start up, to support this requirement the previous update added the 
RegistryLookup PathComponent. This update builds upon that by adding a 
Steam.exe PathReference asset which locates the Steam.exe using the Windows 
Registry via the RegistryLookup PathComponent.

To improve the coverage and usability of the BepInEx Template, the template now
includes a SteamBepInExLaunch Pipeline and the Launch Pipeline was renamed to 
BepInExLaunch

In Order to use the SteamBepInExLaunch Pipeline, copy it over the BepInExLaunch 
pipeline in your Build and Launch Pipeline, or replace BepInExLaunch anywhere 
you used it with the SteamBepInExLaunch pipeline.

References to the Launch Pipeline will be automatically updated to 
BepInExLaunch.

### Templates

Two new Templates have been added to ThunderKit under 
`ThunderKit/Editor/Templates`
DirectLaunch is a new pre-configured Launch pipeline which directly executes
the game's Executable in its working directory. SteamLaunch is a new 
pre-configured launch pipeline which executes the game's Executable in its 
working directory using Steam and the applaunch command line argument.

### GameAppId and Steam Launching

In order to use any of the pre-configured Steam launching pipelines you will 
need to provide ThunderKit with the games Steam App Id.

Follow these Steps to setup Steam Launching
1. Create a new PathReference under your Assets directory and name it GameAppId
2. Add the Constant PathComponent to the newly created PathReference
3. Find the Steam App Id for the game you're modding
  * You can find the SteamAppId easily by copying it from the game's Steam 
    Store page url

After Completing these steups you're seting to use Steam Launching.

### Fixes and Improvements

* Recently a ManifestIdentity data loss issue was re-discovered, this has not
  yet been resolved but some pieces of code have been updated in response while
  a proper resolution is pending
* Some PipelineJobs and PathComponents referenced a Manifests's 
  ManifestIdentity by using LINQ to query for it in the Manifest's Data 
  elements, these have been updated to use the Manifest's cached Identity
  reference.
* StageThunderstoreManifest has been updated to provide a clearer error 
when a dependency is missing its ManifestIdentity
* Some minor code cleanup was done against commented out code and unnecessary
  usings
* Fixed LocalThunderstoreSource not updating its listing when it already has 
  Packages listed
* Fixed PackageSource.Clear Failing to clear Packages successfully under some
  conditions
* Fixed PackageManager failing to render correctly when a Package has invalid 
  dependency information



## 3.2.0

### New Feature: PathComponent: Registry Lookup

the Registry Lookup path component has been added to support cases where values
from the registry are needed to complete Pipeline efforts. For example, some 
games require steam to execute them, and you may need to do so using the Steam 
executable. The installed Steam executable can easily be located by looking up 
the value in the registry, and this provides a path to that.

### Performance Improvements

* ThunderstoreAPI has been updated to utilize gzip compression in order to 
  greatly increase the speed of acquiring package listings.
* Fixed MarkdownRenderer's LinkInlineRenderer leaking handles and memory
* Fix an issue with SettingsWindowData and ThunderstoreSettings that would cause
  the settings window to have poor responsiveness

### Bugs

* Fix a pageEntry.depth to be pageEntry.Depth in Documentation.cs for Unity 
  2019+
* Fix a NullReferenceException that could show up on the Settings window 
  sometimes
* Fix cases in PackageManager that could cause a null reference and fail to 
  load the manager UI
* Fix a problem that would cause a PackageSource to have no packages and be 
  unable to be populated

## 3.1.7

### Fixes and Improvements

* Improve the Package Manager search responsiveness

## 3.1.6

### Fixes and Improvements

* Improve package import process with better ProgressBar status messages
* Install packages as final step of import process

## 3.1.1

This update implements support for .NET 3.5 and includes a number of general 
improvements and fixes

### Optimization

Special thanks to Therzok for doing an optimization pass to clean up a number 
of cases where the code could be more efficient and cleaner.

### Package Manager
* Now updates when package sources are refreshed.
* Now refreshes package sources when opened.
* Now has a Refresh button next to the Filters button which will refresh all 
  available PackageSources
* PackageSources now invoke the SourcesInitialized event when a source has been
  updated
* PackageSources can register an event handler on the InitializeSources event 
  to be informed when it should update
* Thunderstore API no longer automatically updates on a timer

### 3.5 Migration changes

* Added csc.rsp and mcs.rsp to AssemblyDefinition containing folders to ensure 
  that the correct language version is used for ThunderKit regardless of 
  Scripting Back End choice.
* Removed Async/Await as its not available in 3.5
  * Some cases were replaced with other asynchronous mechanisms.
  * More cases will be moved to asynchronous mechanisms in the future, however 
    there are currently a few that were migrated to synchronous execution.
* Migrate to Directory.GetFiles and Directory.GetDirectories over 
  Directory.EnumerateFiles and Directory.EnumerateDirectories due to lack of 
  support in .NET 3.5


### Zip Changes

* Migrated to SharpCompress from System.IO.Compression
* Updated zip handling in ThunderstoreSource and LocalThunderstoreSource
* Updated Zip PipelineJob to use SharpCompress

### Markdown / Documentation changes
* Improved method of locating Documentation assets
* Significant improvements made to the UIElementRenderer allocations
* Improvements to Regex Usage

### File System changes
* Migrated many file system management facilities to use FileUtil instead of
  System.IO types
* Updated Copy PipelineJob to use FileUtil  This changes how Copy works, A 
  recursive copy will Replace the designated destination directory, not fill 
  its contents
* Stage Manifest Files now uses FileUtil to deploy files

### BepInEx Template Changes

The BepInEx template has been updated to use some new features that arose out 
of the .NET 3.5 changes. First, the template is somewhat large so it has been 
broken up into 4 Pipelines
1. Stage:  This pipeline executes StageAssetBundles, StageAssemblies, 
   StageDependencies, StageManifestFiles and finally StageThunderstoreManifest.
2. Deploy: Conducts the following Copy jobs
    1. Copy BepInEx to ProjectRoot/ThunderKit/BepInExPack
    2. Copy plugins to ProjectRoot/ThunderKit/BepInExPack/BepInEx/plugins
    3. Copy patchers to ProjectRoot/ThunderKit/BepInExPack/BepInEx/patchers
    4. Copy monomods to ProjectRoot/ThunderKit/BepInExPack/BepInEx/monomod
    5. Copy winhttp.dll to the Games root directory
    6. Copy a BepInEx config targeted by the PathReference named BepInExConfig 
       if it is defined in the project to 
       ProjectRoot/ThunderKit/BepInExPack/BepInEx/config
3. Launch:  Executes the games executable with the necessary command line 
   parameters to load BepInEx
4. Rebuild and Launch:  Executes the 3 prior Pipelines in order.

To get started on a new mod project you only need to copy the 
`Rebuild and Launch` pipeline into your Assets folder and then populate the 
Manifest field.

## 3.0.0 

### Initial Setup
A Welcome window has been added to ThunderKit to help users set up their 
project. This window can be disabled by a toggle it provides.

### ThunderKit Installer - Removed
The ThunderKit installer has been removed.  The installer caused many 
development issues and lost work during the development of ThunderKit. While 
this issue may not have affected end users, the risk associated with the cost 
of lost work makes this feature dangerous to continue to maintain.

Unity 2018.1-2019.2 users will need to add the Thunderkit dependency to their 
projects Packages/manifest.json

For Unity 2019.3+ users can add ThunderKit using the Git url and use the 
[Install from Git](https://docs.unity3d.com/2019.3/Documentation/Manual/upm-ui-giturl.html)
option in the Unity Package Manager.

### ThunderKit Settings

ThunderKit Settings now get a dedicated window from ThunderKit and can be 
accessed from the main menu under 
[Tools/ThunderKit/Settings](menulink://Tools/ThunderKit/Settings). These 
settings will no longer show up in the Project Settings window.

### Debugging Features

ComposableObjects now support some debugging features to provide an easy access
interface to implementations of Composable Object to report errors in the UI.

ComposableElements now have 2 members, IsErrored and ErrorMessage. The 
ComposableObjectEditor will change the header color of ComposableElements to 
red if IsErrored is true, signalling where a problem may be.

Implementations of ComposableObject are responsible for setting the values in 
IsErrored and ErrorMessage.  

For examples see [Pipeline](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Pipelines/Pipeline.cs) 
and [PathReference](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Core/Paths/PathReference.cs)

If a pipeline encounters a problem it will halt its execution and highlight the
step that it faulted on. The PipelineJobs and Pipeline itself are setup to log 
exceptions to the Unity console like normal, with these two pieces of 
information you should be able to quickly identify and rectify problems.

ThunderKit Manifests do not utilize these debugging features as they are only 
Data Containers, however if worth while usage for debugging issues can be 
identified then support will be added.

### Dependencies

Dependency Management in the 3.0.0 update has changed significantly.  Instead 
of Manifests installing and managing dependencies in its entirety, Manifests 
will now only be responsible for containing dependency references.  

Instead a user will now install packages via the 
[Package Manager](menulink://Tools/ThunderKit/Packages), and then 
add the Manifest from the Packages folder to the Manifest that requires the 
dependency

### Package Manager

ThunderKit now includes a complete Package Manager, available from the main 
menu under [Tools/ThunderKit/Packages](menulink://Tools/ThunderKit/Packages)

The ThunderKit Package Manager is how you will add and remove all mod 
dependencies for your project.
If a mod in your project needs to depend on a Mod, Loader, or Library, you have
the ability to install these dependencies through the Package Manager.

Currently the Package Manager comes with support for Thunderstore by default, 
select your Thunderstore community by setting the url from the 
[ThunderKit Settings](menulink://Tools/ThunderKit/Settings).

You can also create a Local Thunderstore source where you can specify a folder 
to examine for zip files. Zip files in Local Thunderstore Sources must conform 
to Thunderstore's file naming schemes in order to be resolved correctly. 

This scheme is: `Author-ModName-Version.zip`

### Documentation
Documentation is a major issue for new users and as such ThunderKit now comes 
with integrated documentation to help onboard new users. The documentation 
available from the main menu under Tools/ThunderKit/Documentation

Documentation is a work in progress and improvements will be made as a better 
understanding is gained about users needs for information.

## 2.2.1
  * Fix issues with assembly identitification

## 2.1.3
  * New Features
    * Establish base for documentation system
    * Establish Package management as a core system
    * Add support to drag and drop Thunderstore package zip files into 
      ThunderstoreManifest dependencies
    * Components of ComposableObjects now provide Copy, Paste, and Duplicate 
      from their menus
    * Ensure a Scripting Define is always added for packages installed by 
      ThunderKit, Define will be the name of the package
  * Improvements
    * Clean and organize systems for managing the loading process of ThunderKit
    * Improve the design of the ThunderKit Installer package to support more 
      versions
    * Use built in Asset Package (unityPackage) options
    * ComposableObject now has an array of ComposableElements instead of 
      ScrtipableObjects
  * Fixes
    * Sort add component options
    * Fix cases where directories are not created when needed
    * Fix some problems with the Thunderstore - BepInEx templates

## 2.1.0 - 2.1.2
  * Fix issues with automatic installer
  * Fix issues with package management

## 2.0.0 - First Major Version update
  * Replace Deployments with new system.
    * Manifest's will now hold all references to files that need to be included
      or processed for deploying a mod
    * Deployment operations will now be handled by Pipeline's and Pipeline Jobs
    * Pipelines are containers for pipeline jobs, pipelines with special 
      requirements can be made by creating derivatives of Pipeline.

## Early Versions
* 1.x.x - untracked iterative updates to ThunderKits feature set
* 1.0.0 - Initial Relesae of Thunderkit
