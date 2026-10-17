# Development and recovery

Updated: 2026-10-17. This is the current resume entry point; dated design and test records describe their own candidate, not competing current instructions.

## Current state

**Building Planner** is the adopted display name and English HUD title. Stable technical identity remains CraftingPlanner: GUID, namespace, installed filename, package and repository identity.

The **0.1.16** visual baseline is user-accepted, including repair energy above the planner quantity/arrows. Installed **0.1.17** corrects only the subsequently reported multi-line native-cost overlap and awaits game acceptance. Core actions have been exercised with mouse/keyboard and gamepad; ordinary HUD appearance across all 11 host languages is user-reported from 0.1.11. This is bounded runtime evidence, not exhaustive functional coverage or stable-release acceptance.

The passive PlacementCommitProbe returned its native attempt evidence and was removed from loading after exact backup. Building Planner 0.1.17 was installed with the game closed; previous 0.1.16/log and observer payload are preserved privately. Installation receipt is `.local/evidence/pre-0.1.17/installation.json`. Goals remain session-local.

A new screenshot reopens only multi-line native spend overlap in a world-clearing row. The user adopted a separate native-cost column to the left of planner quantity on 2026-10-17, shifting ingredients left while preserving row height and accepted ordinary/single-energy rows. Implementation is on dev/0.1.17; native-reference compilation in both modes, clean hosted build and exact downloaded-DLL policy checks pass. Verify the reported clearing row, ordinary/single-energy rows and reopen/quantity edits for drift; native visual acceptance remains pending.

## Exact installed artifact

| Identity | Value |
| --- | --- |
| Development branch | `dev/0.1.17` |
| Frozen runtime source | `50652ee3e8ecc3afbabc26ae06f407228e28b6ce` |
| Version | `0.1.17` / assembly `0.1.17.0` |
| CI run / artifact | `39022244129` / `12096483899` |
| DLL SHA256 | `5db81e4f0ecdf7527399564135790be8aa64887890dd6986fa0893f229b81343` |
| ZIP SHA256 | `0c0f96ba23bb12c9af3f9d8a6221befe62706dae150c0f47689c6baf8577113a` |
| Calibration | enabled; F2 format 8, 55 individually resettable sliders |

Exact CI bytes are retained under ignored `artifacts/CraftingPlanner-0.1.17/`; previous payloads and installation receipts remain private under `.local/`. Numbered artifacts are immutable. Later documentation commits do not change this frozen runtime identity. No 0.1.17 installed-runtime log is claimed yet.

Both native-reference configurations, clean hosted build, state/geometry/font/calibration/localization/invalidation/energy-layout checks, exact-DLL tests and package boundaries passed. Accepted cases and limits are owned by [TEST_LOG.md](TEST_LOG.md). The live acceptance snapshot is `.local/evidence/accept-0.1.16-visual/LogOutput.log`, SHA256 `96ae57df373be5e2137e26f9e87678ddac37a51ecdccccde85c7a66570c5316d`: it loads 0.1.16 with no planner exception/disable found. It is a partial running-session snapshot, not a completed shutdown log.

## Next development step

Automatic reduction of a fulfilled goal is required and **absent from 0.1.16**. For repeatable placeables, the user chose successful placement/material commitment: decrement exactly one, remove at zero, and recompute aggregate requirements. Failed/cancelled placement removes none. Native code alone spends materials. Repairs/upgrades/clearing require their own verified success boundary and identity; do not infer completion from menu selection or arbitrary inventory changes.

Start with existing evidence in [VERIFIED_GAME_DATA.md](VERIFIED_GAME_DATA.md), private `.local/build-spend-stage.txt`, and shared GraveyardKeeperResearch. `BuildModeLogics.DoPlace` has early returns and discards the material-removal result; a generic postfix is not yet sufficient proof of success. Close success/identity/exactly-once gates before production implementation. Verify success, cancellation, rejection and repeated actions on a meaningful candidate; research alone does not consume a numbered version.

Placement research is recorded in [PLACEMENT_COMMIT_RESEARCH.md](PLACEMENT_COMMIT_RESEARCH.md). The installed passive probe0.1.0 returned two exact successful repeatable placements, a busy-space rejection, standalone cancellations and upstream insufficient-resource rejection. Raw log and bounded results are preserved; production decrement remains absent. After the agreed multi-line UI correction, select the smallest production success predicate from these facts, verify exact goal matching/once-only reduction/zero removal/derived refresh, then investigate separate upgrade/repair/clearing boundaries. Do not repeat the observed placement probe matrix merely to recover evidence.

After this milestone, perform a focused functional/release pass, including carried-bag and capacity edges only where correctness remains unproven. The fate of temporary F2 calibration in a public build is still a product decision; neither removing it nor building a permanent settings system is adopted. Per-save persistence, reserve protection, recursive material planning and remote/multiple-storage logistics remain deferred. Main/stable promotion needs separate explicit acceptance.

## Resume and document ownership

Read DevRules `WORKING_RULES.md` / `CODEX_WORKFLOW.md`, repository [AGENTS.md](../AGENTS.md), then:

- [PRODUCT_SCOPE.md](PRODUCT_SCOPE.md): active mechanics, accepted UX, deferred choices; its dated design history is explicitly historical.
- [VERIFIED_GAME_DATA.md](VERIFIED_GAME_DATA.md): native seams, evidence and remaining host questions.
- [TEST_LOG.md](TEST_LOG.md): immutable artifact identities and bounded runtime acceptance, chronologically preserved.
- [UI_IMPLEMENTATION.md](UI_IMPLEMENTATION.md): current component map and historical engineering gates.
- [RESEARCH_REPORT_2026-09-21.md](RESEARCH_REPORT_2026-09-21.md): initial historical research, superseded by later evidence where indicated.

Inspect actual Git status, unpublished changes and current remote state. Keep one writer per checkout. Before fresh host research, consult shared `GraveyardKeeperResearch/docs/RESEARCH_INDEX.md` and its linked knowledge. Reusable native transfer/tip-style findings were promoted there at commit `70320d9`; product and acceptance ownership stays here. Do not repeat the accepted HUD-anchor experiment or add Borderless compensation without new evidence.

## Build and local helpers

The project targets .NET Framework 4.7.2 / C# 7.3; hosted builds use SDK 8. `Plugin.PluginVersion` owns semantic version and derives assembly/file identity. [build.yml](../.github/workflows/build.yml) owns hosted validation/packaging. Compile-reference stubs are signatures only and must not leak into distributable output.

```powershell
dotnet restore CraftingPlanner.csproj --configfile nuget.config
dotnet build CraftingPlanner.csproj -c Release --no-restore --nologo
```

Temporary calibration requires `-p:PlannerCalibration=true`; otherwise the F2 editor and its guards compile out. Hosted candidate CI explicitly records this flag. Output is `bin/Release/CraftingPlanner.dll`; compilation alone does not establish runtime acceptance or exact handed artifact identity.

Private `.local/compile-native.ps1` verifies against installed host references (default calibration, `-Production` without it). `.local/REFERENCE_BUILD_NOTE.md` records an unresolved local full-SDK/cache issue: stale reference outputs must not be treated as fresh success. Native-reference compilation and clean hosted builds are the established verification paths. Private reflection helpers, logs, proprietary inputs and machine installation configuration stay ignored; consult existing `.local/` evidence before recreating them. No code/build/install work is part of this documentation handoff.
