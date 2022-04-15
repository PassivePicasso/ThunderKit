---
{ 
	"title" : "Zip",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Pipeline_2X_Icon" ]
}

---

[Zip](assetlink://GUID/8808e815b3ce9c94e9552cb1dee9e305) provides the ability to zip a directory

## Fields

* **Compression**
  - Sets the compression level of the zip
* **IncludeBaseDirectory**
  - Includes the Source directory in the zip
* **Source**
  - Path Folder to zip
  - Supports PathReferences
* **Output**
  - Name of zip file including extension
  - Supports PathReferences

## Inherited Fields
* **Per Manifest**
  - When enabled this job will execute once for each Manifest associated with the Pipeline
* **Excluded Manifests**
  - When Per Manifest is toggled on and you need the pipeline to not execute this job for certain Manifests, add them to this field

## Remarks

PathReferences are resources which can define dynamic paths, you can use them in fields that support PathReferences by invoking them with arrow brackets.


The [BepInEx Package Pipeline](assetlink://Packages/com.passivepicasso.thunderkit/Editor/Templates/BepInEx/Pipelines/Package.asset) uses Zip as a final publishing step.