---
{ 
	"title" : "ExecutePipeline",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconClasses" : [ "header-icon", "TK_Pipeline_2X_Icon" ]
}

---

[ExecutePipeline](assetlink://GUID/50df4f9027e15e04b931a8d460fb22c5) will invoke an assigned Pipeline

## Fields
* **Execute Pipeline**
  - A Pipeline to invoke
* **Inherit Root Manifest**
  - When toggled on the assigned Pipeline will have its Manifest field replaced with the parent Pipeline's assigned Manifest

## Remarks

`Warning` There is no cycle detection in place, take care to avoid making infinite loops

Use this to invoke additional pipelines from the current pipeline.

Using Execute Pipeline you can organize complex Pipelines into smaller re-usable chunks