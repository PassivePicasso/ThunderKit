# ProjectSettings import fixtures

[ImportProjectSettingsTests](../../ImportProjectSettingsTests.cs) runs the real
`ImportProjectSettings.ExportProjectSettings` against a captured `globalgamemanagers`
for each Unity version, validating the export under the running editor.

These `globalgamemanagers` files are **generated**, not hand-committed. The
[generate-project-settings-fixtures](../../../../.github/workflows/generate-project-settings-fixtures.yml)
workflow (manual `workflow_dispatch`) builds a throwaway empty player with each
Unity version in the test matrix, extracts the `globalgamemanagers` the build
produced, and commits it here as:

```
GlobalGameManagers/<exact-unity-version>/
  globalgamemanagers   # Git LFS
  fixture.json         # { "unityVersion": "<exact-unity-version>" }
```

The test auto-discovers every such folder. With one fixture per matrix Unity
version, each editor's CI job exercises the export against a `globalgamemanagers`
built by that exact editor — an exact class-data match, which is ThunderKit's
real "editor matches the game" import scenario.

To (re)generate: run the workflow from the Actions tab. Nothing here needs to be
created by hand.
