---
{ 
	"title" : "ExecuteProcess",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Pipeline_2X_Icon" ]
}

---

[ExecuteProcess](assetlink://GUID/77f65d4371163fb4695da79ab8df0e84) starts a new process

## Fields
* **Working Directory**
  - The Working Directory the process should run under
  - Supports PathReferences

* **Executable**
  - The executable to start
  - Supports PathReferences

* **Arguments**
  - An array of command line arguments to pass to the process being started
  - Supports PathReferences

## Remarks

Execute Process can be used to launch games, external build processes, or any other process necessary for a build pipeline

Use PathReferences to simplify the fields of ExecuteProcess and to provide a centralized set of variables to make it easier to manage multiple build pipelines.

The [BepInEx Launch Pipeline](assetlink://GUID/bee6483f5bcf7054b86d13321eef27e5) uses Execute Process to launch the configured game and pass parameters necessary to load BepInEx with winhttp.dll and doorstop
