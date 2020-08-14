namespace PassivePicasso.ThunderKit.Editor
{
    public static class ConfigTemplate
    {
        public static string CreatBepInExConfig(bool consoleEnabled, string logLevel) =>
            string.Format(Content, consoleEnabled.ToString().ToLower(), logLevel);

        public static readonly string Content = @"[Caching]
## Enable/disable assembly metadata cache
## Enabling this will speed up discovery of plugins and patchers by caching the metadata of all types BepInEx discovers.
# Setting type: Boolean
# Default value: True
EnableAssemblyCache = true

[Logging]

## Redirects text from Console.Out during preloader patch loading to the BepInEx logging system.
# Setting type: Boolean
# Default value: True
PreloaderConsoleOutRedirection = true

UnityLogListening = true

[Logging.Console]

## Enables showing a console for log output.
# Setting type: Boolean
# Default value: False
Enabled = {0}

## If true, console is set to the Shift-JIS encoding, otherwise UTF-8 encoding.
# Setting type: Boolean
# Default value: False
ShiftJisEncoding = false

DisplayedLogLevel = {1}

[Logging.Disk]

WriteUnityLog = true

AppendLog = false

Enabled = true

DisplayedLogLevel = {1}

[Preloader]

## Enables or disables runtime patches.
## This should always be true, unless you cannot start the game due to a Harmony related issue (such as running .NET Standard runtime) or you know what you're doing.
# Setting type: Boolean
# Default value: True
ApplyRuntimePatches = true

## If enabled, basic Harmony functionality is patched to use MonoMod's RuntimeDetour instead.
## Try using this if Harmony does not work in a game.
# Setting type: Boolean
# Default value: True
ShimHarmonySupport = true

## If enabled, BepInEx will save patched assemblies into BepInEx/DumpedAssemblies.
## This can be used by developers to inspect and debug preloader patchers.
# Setting type: Boolean
# Default value: False
DumpAssemblies = false

## If enabled, BepInEx will load patched assemblies from BepInEx/DumpedAssemblies instead of memory.
## This can be used to be able to load patched assemblies into debuggers like dnSpy.
## If set to true, will override DumpAssemblies.
# Setting type: Boolean
# Default value: False
LoadDumpedAssemblies = false

## If enabled, BepInEx will call Debugger.Break() once before loading patched assemblies.
## This can be used with debuggers like dnSpy to install breakpoints into patched assemblies before they are loaded.
# Setting type: Boolean
# Default value: False
BreakBeforeLoadAssemblies = false

[Preloader.Entrypoint]

## The local filename of the assembly to target.
# Setting type: String
# Default value: UnityEngine.CoreModule.dll
Assembly = UnityEngine.CoreModule.dll

## The name of the type in the entrypoint assembly to search for the entrypoint method.
# Setting type: String
# Default value: Application
Type = Application

## The name of the method in the specified entrypoint assembly and type to hook and load Chainloader from.
# Setting type: String
# Default value: .cctor
Method = .cctor
";
    }
}