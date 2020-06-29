## ThunderKit
ThunderKit is a Mod kit construction toolset with Thunderstore.io integration for Unity3d games, providing the bare essentials for getting started with making Mods for Unity games to publish on Thunderstore.

### Setup

#### Unity 2017 and lower (Currently not supported, but support is being investigated)
1. Start a new Unity3d Project 
1. Download this repository as a zip 
1. Extract it into your project's Assets folder.
1. Unity will attempt to compile and fail, this is expected
1. Close Unity and re-open the project

#### Unity 2018.1 through 2019.2
 Start a new Unity3d Project and add this to your Packages/manifest.json dependencies array;
```json
    "com.passivepicasso.thunderkit": "https://github.com/PassivePicasso/ThunderKit.git",
```

#### Unity 2019.3+
  Start a new project, open the package manager and Add with Git the git link;
```
https://github.com/PassivePicasso/ThunderKit.git
```
