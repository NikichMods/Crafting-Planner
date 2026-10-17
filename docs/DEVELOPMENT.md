# Development and recovery

Updated: 2026-10-17. This is the current resume entry point; dated design and test records describe their own candidate, not competing current instructions.

## Current state

**Building Planner** is the adopted display name and English HUD title. Stable technical identity remains CraftingPlanner: GUID, namespace, installed filename, package and repository identity.

The **0.1.16** visual baseline is user-accepted. The user reports installed **0.1.17** technically separates the multi-line native costs and requests a unified adjustable block, including single energy, plus bare centred quantities, measured arrow gaps and cap20 per repeatable goal. Implementation is on `dev/0.1.18`; final positions await user calibration. Core actions have been exercised with mouse/keyboard and gamepad; ordinary HUD appearance across all 11 host languages is user-reported from 0.1.11. This is bounded runtime evidence, not exhaustive functional coverage or stable-release acceptance.

The passive PlacementCommitProbe returned its native attempt evidence and was removed from loading after exact backup. Building Planner 0.1.18 is installed with the game closed; previous 0.1.17/log are preserved privately. Installation receipt is `.local/evidence/pre-0.1.18/installation.json`. Goals remain session-local.

0.1.18 adds F2 cost-block X/Y, real signed material offsets relative to that block and common glyph-edge arrow gap(default13). Single-energy stacking is deliberately superseded; the rightmost menu column is exclusively the centred numeral. Quantity is bare in menu and shared card; all edit paths use a central repeatable maximum20/one-off maximum1. F2 format 9 has 58 resettable sliders and may open in supported Build with no pinned goals. Native-reference calibration/production builds, clean hosted build, compiled policy/state checks and exact downloaded-DLL tests pass. Real rendering/tuning remain pending: user adjusts cost/material X/Y, checks number/arrow centring at0/1/9/10/20 and menu/HUD cap20, then exports format 9. Adopt returned positions as defaults only after that user tuning; handed0.1.18 is immutable.

## Exact installed artifact

| Identity | Value |
| --- | --- |
| Development branch | `dev/0.1.18` |
| Frozen runtime source | `23910f94bc1118dbb64a4b55fa466584981f1419` |
| Version | `0.1.18` / assembly `0.1.18.0` |
| CI run / artifact | `39024756811` / `12098465914` |
| DLL SHA256 | `faeba2d7538196b8975d61344268d8dc03293d01874231c01154832a4721a7ef` |
| ZIP SHA256 | `6da6855bfca73f7606809723d1e19429e0e7aedff2a94ed6d90b9ea388f3f45e` |
| Calibration | enabled; F2 format 9, 58 individually resettable sliders |

Exact CI bytes are retained under ignored `artifacts/CraftingPlanner-0.1.18/`; previous payloads and installation receipts remain private under `.local/`. Numbered artifacts are immutable. Later documentation commits do not change this frozen runtime identity. No 0.1.18 installed-runtime log is claimed yet; 0.1.17's technical separation return is recorded in TEST_LOG.md.

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
