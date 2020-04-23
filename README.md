## Warning: this readme is currently significantly out of date and will be updated soon (4/23/2020)

## Rain of Stages
Rain of Stage is a mod and starter pack for building custom stages for Risk of Rain 2

It is not recommended to publish mods using Rain of Stages at this time as its still under heavy preliminary development and may undergo significant changes before its final release.

### Easy Sandbox setup

Rain of Stages is split into 2 parts, 
Rain of Stages depends on BepinEx, MMHook, and Risk of Rain 2's Managed assemblies, you will need to provide these to the projects in order to compile them.  The preparation step below provides an easy way to accomplish this, while sandboxing a version of Risk of Rain 2 to work against.
The Projects are already setup with the References they require so check your References for warnings to see what assembly references you're missing.

#### Preparation

 Make a copy of your Risk of Rain 2 folder and place it in the same parent folder you're going to clone the repository under.
This will allow the RainOfStages Plugin solution to automatically refernce all the Risk of Rain 2 libraries.

If you followed the Bepinex installation directions and ahve R2API already installed they should also load automatically, however directory structure differences could prevent it.  

### Setup
Rain of Stages is split into 2 parts, 
Rain of Stages depends on BepinEx, MMHook, and Risk of Rain 2's Assembly-CSharp you will need to provide these to the projects in order to compile them.
The Projects are already setup with the References they require so check your References for warnings to see what assembly references you're missing.

 2. Clone this repository 
	* [Open in Visual Studio](git-client://clone?repo=https://github.com/PassivePicasso/Rain-of-Stages)
	* [Open in Desktop](github-windows://openRepo/https://github.com/PassivePicasso/Rain-of-Stages)
	* [Download Zip](https://github.com/PassivePicasso/Rain-of-Stages/archive/master.zip)
3. Open repository root directory with Unity 3D 2018.3.13f
4. Add RoR2 Assembly-CSharp.dll into any directory under Assets\
5. In Unity, locate Assembly-CSharp.dll in the project and select it.
6. Using the Inspector un-check Validate Reference then click Apply ![Using the Inspector un-check Validate Reference then click Apply ](https://i.imgur.com/2JywInT.png)
7.  Select Assets\RainOfStages.Shared\RainOfStages.Shared assembly definition
    * In Inspector add an Assembly Reference and Select Assembly-CSharp.dll
    * ![Select RainOfStages.Shared Assembly Definition](https://i.imgur.com/xeztYI1.png)
    * ![Add Assembly-Csharp reference](https://i.imgur.com/ABVeKvS.png)

8. Open RainOfStages\RainOfStages.sln in Visual Studio

9. If you conducted the preparation step at the beginning of this process, then the required assemblies should be referenced successfully.  Verify if all references loaded correctly and then build the project and skip the next step.
9. Fix references in project RainOfStages.Plugin by adding the following assembly references
	* All DLL files in the Risk Of Rain Data Folder
	* MMHook_Assembly-CSharp.dll
		*  This is included with the R2API
	* BepInEx.dll
	* RainOfStages.Shared.dll 
		*  If you launched unity and correctly setup the project then new folders will be generated under the repository root folder, navigate to Library\ScriptAssemblies to find RainOfStages.Shared.dll
