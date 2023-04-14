---
{ 
	"title" : "Import Extensions",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_ThunderKitSetting_2X_Icon" ]
}

---

ThunderKit's game import process is extensible, providing the ability to rework
the entire import process to suit your projects needs.  

Import extensions can be created to import Unity Packages, ThunderKit Packages,
Import additional game files, modify assemblies before import, and much more.

ThunderKit comes with some base classes for some of these processes.

Adding a new import extension is simple, follow this short guide to make an 
importer which simply logs "Hello World" to the unity console at the beggining 
of the import process

Use the following steps to get started;

1. Create a new Assembly Definition to contain your Import Extensions.
2. Select the Assembly Definition and add a reference to [ThunderKit.Core](assetlink://GUID/280569ac301428d4d950fdf647900922)
3. Add a new C# Script to the Assembly Definition named "AssemblyInfo.cs"
   and open it
4. Remove all the content of the new script file
5. Add `[assembly: ThunderKit.Core.Config.ImportExtensions]` 
   to the script file and save it.
6. Add another new C# Script file to the Assembly Definition and change the 
   contents to:
   ```cs
    using ThunderKit.Core.Config;
    using UnityEngine;

    namespace HelloWorldImporter
    {
        public class HelloWorldImporter : OptionalExecutor
        {
            public override int Priority => int.MaxValue;

            public override string Description 
            {
                get 
                {
                    return "Logs Hello World";
                }
            }

            public override bool Execute()
            {
                Debug.Log("Hello world");
                return true;
            }
        }
    }
   ```

#### Fin
Congratulations you have made your first complete custom Importer!

The Execute Method requires you to return true or false.  Return true when your
custom importer has successfully completed its steps.  This allows you to write
importers which can execute multiple times, allowing for compilation or import
processes to complete before continuing.  The import system will wait until the
Unity environment is not busy before executing the next import step, however
sometimes this is not sufficient for more complicated importers.
