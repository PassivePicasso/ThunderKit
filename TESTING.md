# Testing Strategy & Fixture Backlog

This document tracks ThunderKit's automated test coverage: what exists, what's
worth adding, and why. It is the living backlog for EditMode test fixtures.

Tests live in [`Tests/Editor/`](Tests/Editor/) and run via the Unity Test Runner
(NUnit, EditMode). CI runs them across the Unity matrix defined in
[`.github/workflows/main.yml`](.github/workflows/main.yml).

> **What CI gives you, in two parts:**
> 1. **Compile validation** — running EditMode tests forces a clean compile of
>    every assembly in the test assembly's dependency graph
>    (`ThunderKit.Common`, `ThunderKit.Markdown`, `ThunderKit.Core`) on every
>    Unity version. This catches version-specific compile breaks (e.g. the
>    `#if UNITY_6000_5_OR_NEWER` `EndNameEditAction` → `AssetCreationEndAction`
>    split). It does **not** compile assemblies outside that graph — notably
>    `ThunderKit.Addressable.*`, `ThunderKit.Thunderstore`, `ThunderKit.SpaceDock`.
> 2. **Behavioral validation** — only the fixtures below.
>
> **Conditional-compilation caveat:** a `#if UNITY_X` branch is only compiled on
> a matrix version that defines `UNITY_X`. Branches gated on a Unity version absent
> from the matrix are never validated by CI.

Status legend: ✅ done · 🟡 partial · ⬜ not started

---

## Current coverage (existing fixtures)

| Fixture | Covers | Status |
|---|---|---|
| [PackageSourceSettingsTests](Tests/Editor/PackageSourceSettingsTests.cs) | Package-source add/remove + Package Manager window sync | ✅ |
| [PackageHelperTests](Tests/Editor/PackageHelperTests.cs) | `GetCleanPackageName`, `GetStringHashUTF8`, `GetCleanedStringHashUTF8` | ✅ |
| [PathExtensionsTests](Tests/Editor/PathExtensionsTests.cs) | `PathExtensions.Combine` slash normalization | ✅ |
| [PathReferenceTests](Tests/Editor/PathReferenceTests.cs) | `ResolvePath` no-token + unknown-token throws | 🟡 |
| [PackageGroupTests](Tests/Editor/PackageGroupTests.cs) | `HasString`, version indexer, equality | ✅ |
| [PackageVersionTests](Tests/Editor/PackageVersionTests.cs) | Equality / hashset dedup by `dependencyId` | ✅ |
| [EnumExtensionsTests](Tests/Editor/EnumExtensionsTests.cs) | `GetDescription` attribute + fallback | ✅ |
| [UnityPathUtilityTests](Tests/Editor/UnityPathUtilityTests.cs) | `IsAssetDirectory` | ✅ |
| [FileIdUtilTests](Tests/Editor/FileIdUtilTests.cs) | `Compute` determinism / distinctness (MD4) | ✅ |
| [ComposableObjectTests](Tests/Editor/ComposableObjectTests.cs) | `InsertElement` / `RemoveElement` incl. type guard + index mismatch | ✅ |
| [ConstantTests](Tests/Editor/ConstantTests.cs) | `Constant` path component | ✅ |
| [ManifestNameTests](Tests/Editor/ManifestNameTests.cs) | `ManifestName` path component + null fallback | ✅ |
| [SelfDestructingActionAssetTests](Tests/Editor/SelfDestructingActionAssetTests.cs) | Delegate invocation + self-destruct | ✅ |
| [StageThunderstoreManifestTests](Tests/Editor/StageThunderstoreManifestTests.cs) | `RenderJson` round-trip + dependency formatting | ✅ |

---

## Backlog

### Tier 1 — Pure logic, low effort, high value
Deterministic functions, no Editor state. Cheapest wins; run on every Unity version.

- ✅ `PackageHelper.GetCleanPackageName` — [PackageHelper.cs:31](Editor/Core/Utilities/PackageHelper.cs#L31)
- ✅ `PackageHelper.GetStringHashUTF8` / `GetCleanedStringHashUTF8` — [PackageHelper.cs:136](Editor/Core/Utilities/PackageHelper.cs#L136)
- ✅ `PathExtensions.Combine` — [PathExtensions.cs:8](Editor/Common/PathExtensions.cs#L8)
- 🟡 `PathReference.ResolvePath` — [PathReference.cs:24](Editor/Core/Paths/PathReference.cs#L24). Has no-token + unknown-token-throws. **Gap:** multi-token, nested-token, and resolution-cache reuse cases.
- ⬜ `Manifest.EnumerateManifests` — [Manifest.cs:30](Editor/Core/Manifests/Manifest.cs#L30). Assert dependency-graph dedup (HashSet), **cycle tolerance (no stack overflow)**, and throw-on-null Identity/Dependencies/entry.
- ⬜ `VersionIdToGroupId` — [ThunderstoreSource.cs:81](Editor/Thunderstore/ThunderstoreSource.cs#L81), [LocalThunderstoreSource.cs:37](Editor/Thunderstore/LocalThunderstoreSource.cs#L37). `Author-Pkg-1.2.3`→`Author-Pkg`, multi-dash names. **Latent bug:** input with no `-` → `LastIndexOf("-")` is `-1` → `Substring(0,-1)` throws; pin the intended behavior. Logic is duplicated in two sources — candidate to hoist to the base class.
- ✅ `PackageGroup.HasString` + version indexer — [PackageGroup.cs:41](Editor/Core/Data/PackageGroup.cs#L41)
- ✅ `PackageVersion` / `PackageGroup` equality contracts — [PackageGroup.cs:79](Editor/Core/Data/PackageGroup.cs#L79)
- ⬜ `Pipeline.SupportsType` — [Pipeline.cs:301](Editor/Core/Pipelines/Pipeline.cs#L301). `[PipelineSupport]` attribute routing, inheritance, non-job types → false.
- ✅ `EnumExtensions.GetDescription` — [EnumExtensions.cs:12](Editor/Common/EnumExtensions.cs#L12)
- ✅ `UnityPathUtility.IsAssetDirectory` — [Helpers/UnityPathUtility.cs](Editor/Markdown/Helpers/UnityPathUtility.cs)
- ✅ `FileIdUtil.Compute` / MD4 — [FileIdUtil.cs](Editor/Common/FileIdUtil.cs)

### Tier 2 — Editor-coupled, high value (EditMode)
- ✅ `ComposableObject.InsertElement` / `RemoveElement` — [ComposableObject.cs:20](Editor/Core/ComposableObject.cs#L20)
- ✅ `SelfDestructingActionAsset.Action` + cleanup — [SelfDestructingActionAsset.cs](Editor/Core/Actions/SelfDestructingActionAsset.cs)
- 🟡 `PathComponent` leaf classes — [Paths/Components/](Editor/Core/Paths/Components/). Done: `Constant`, `ManifestName`. **Gap:** `GamePath`, `ManifestVersion`, `OutputReference`, `Resolver`, incl. `ManifestIndex == -1` vs valid bounds.
- ⬜ `FlowPipelineJob.Execute` manifest filtering — [FlowPipelineJob.cs:16](Editor/Core/Pipelines/FlowPipelineJob.cs#L16). PerManifest off→once; WhiteList/BlackList filtering; `ManifestIndex` set per-iteration then reset to -1.
- ⬜ `ImportConfiguration` chain (`LoadImportExtensions`, `ImportGame`) — [ImportConfiguration.cs:218](Editor/Core/Config/ImportConfiguration.cs#L218). Executors ordered by `Priority` desc + dedup; index progression, disabled-skip, exception→index=-1, completion→`Cleanup`.
- ⬜ `TransformHierarchyTreeView` construction — [TransformHierarchyTreeView.cs:21](Editor/Addressable/Tools/Controls/TransformHierarchyTreeView.cs#L21). Build in-memory GameObject tree; assert depth/parent/leaf + `transformLookup` id mapping. **Also the only place CI would compile the PR #116 Addressables `EntityId` path** — but only if an Addressables-referencing test assembly exists (see cross-cutting #2).
- ⬜ Thunderstore / SpaceDock `PackagesResponse` parsing — [PackagesResponse.cs](Editor/Thunderstore/PackagesResponse.cs), [SpaceDock/PackagesResponse.cs](Editor/SpaceDock/PackagesResponse.cs). The array-wrap parse (`{"results":[…]}`) and empty-results case. **Parsing is separable from the HTTP fetch** — test the parser; skip the network.

### Tier 3 — Valuable but needs a test seam first
Add `internal` + `[assembly: InternalsVisibleTo("ThunderKit.Core.Tests")]` to unlock these. This **single refactor** also removes the existing fixture's reflection hack.

- ⬜ `PackageSource.EnumerateDependencies` (private) — [PackageSource.cs:253](Editor/Core/Data/PackageSource.cs#L253). Recursive dependency resolution + cycle detection. High value (install correctness).
- ⬜ `PackageSource.LoadPackages` dependency-map building / latest fallback.
- 🛠️ Replace the reflection into private `DeferredRefresh`/`RefreshList` in [PackageSourceSettingsFixture.cs:67](Tests/Editor/PackageSourceSettingsFixture.cs#L67) with the same seam — removes a latent break (renaming those privates currently breaks pre-2021 jobs).

### Tier 4 — Defer to integration / skip
Filesystem-driven path components (`FindFile`/`FindDirectory`), pipeline jobs
(`Copy`/`Delete`/`ExecuteProcess`), `BatchModeExecutePipeline` arg parsing,
`FixMisreferencedAssets` (YAML + reflection), live network fetches, and
global-state side-effect code (`ScriptingSymbolManager`, tree-view click
handlers). HARD to isolate; only worth it as integration tests later.

---

## Cross-cutting recommendations

1. **One `InternalsVisibleTo` seam** unlocks all of Tier 3 *and* cleans up the
   existing fixture's reflection — highest-leverage single change.
2. **A "compile-everything" test assembly** referencing
   `ThunderKit.Addressable.*`, `ThunderKit.Thunderstore`, and
   `ThunderKit.SpaceDock` (even with one trivial `[Test]`) turns CI into a full
   compile gate. Today those assemblies are **never compiled** by CI, so a
   version-specific break in them (e.g. the PR #116 `TransformHierarchyTreeView`
   `EntityId` changes) would ship undetected.
3. **Re-enable code coverage** once GameCI's coverage package stops failing to
   compile on newer Unity (it currently uses the obsolete non-generic `TreeView`
   API; see the `coverageOptions: ''` note in the workflow). Then gate coverage
   with `assemblyFilters:+ThunderKit.*` for a real coverage number.
