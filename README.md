
## Rain of Stages
Rain of Stage is a mod and starter pack for building custom stages for Risk of Rain 2

It is not recommended to publish mods using Rain of Stages at this time as its still under heavy preliminary development and may undergo significant changes before its final release.

### Setup
Rain of Stages is split into 2 parts, 
Rain of Stages depends on BepinEx, MMHook, and Risk of Rain 2's Assembly-CSharp you will need to provide these to the projects in order to compile them.
The Projects are already setup with the References they require so check your References for warnings to see what assembly references you're missing.

 1. (Preparation) Locate dependencies: Bepinex.dll MMHook_Assembly-CSharp.dll and Assembly-CSharp.dll
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

9. Fix references in project RainOfStages.Plugin by adding the following assembly references
	* Assembly-CSharp 
		*  this is the Assembly-CSharp.dll distributed with Risk Of Rain 2
	* MMHook_Assembly-CSharp 
		*  I don't know where to find this, but you need it.
	* BepInEx 
		* This is the mod package, assume compatibility with latest released version
	* RainOfStages.Shared.dll 
		*  If you launched unity and correctly setup the project then new folders will be generated under the repository root folder, navigate to Library\ScriptAssemblies to find RainOfStages.Shared.dll
