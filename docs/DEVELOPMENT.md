# Development and recovery

Updated: 2026-10-17. This is the current resume entry point; dated design and test records describe their own candidate, not competing current instructions.

## Current state

Building Planner keeps stable technical identity CraftingPlanner. Visual baseline **0.1.19**, explicitly accepted, restores both user-tuned 0.1.18 layouts independently: materials without native costs29/0, with native costs46/0; common native cost24/-1, counter17/1 and fine-arrow X8/-9. Profile choice follows actual visible native cost-block presence per row, including mixed menus. Right increase arrows hide at repeatable20/one-off1 in menu and shared HUD, and return below the bound without moving the numeral.

F2 format10 retains60 individually resettable sliders. The user accepts0.1.19 independent settings,20/19 arrow behavior and visuals. TEST_LOG.md owns the bounded acceptance and log identity; no further visual tuning is requested. Ordinary HUD across all11 languages is user-reported from0.1.11; gamepad/all11-language functional acceptance remains open. This is not stable-release acceptance.

Installed dev/0.1.20 implements repeatable-placement decrement and preserves the accepted visual baseline. Exact candidate build/installation checks and the returned bounded native placement pass are verified (TEST_LOG.md). No repairs/upgrades/clearing completion is inferred.

## Exact installed artifact

| Identity | Value |
| --- | --- |
| Development branch | `dev/0.1.20` |
| Frozen runtime source | `db04213bfed06560fc4cc188f1ca2d73e2b884df` |
| Version | 0.1.20 / assembly0.1.20.0 |
| CI run / artifact | `39053843624` / `12108657880` |
| DLL SHA256 | `e45dd14fa6a301e7d887811a7008aa107edca1e5bc7bafdd0cb71839d9396058` |
| ZIP SHA256 | `466f5862e50463d2f88b579a87f3f6b0969080d643edb7b7360ec32e05b26961` |
| Calibration | enabled; F2 format 10, 60 individually resettable sliders |

Exact CI bytes are retained under ignored `artifacts/CraftingPlanner-0.1.20/`; previous payloads and installation receipts remain private under `.local/`. Numbered artifacts are immutable. Later documentation commits do not change this frozen runtime identity. The0.1.19 acceptance snapshot is recorded in TEST_LOG.md; its hash is023e04bcdf165d7f9c69f78b2f336ae1ac9a9bd8460660766a456b772f5cb51a.

Both native-reference configurations, clean hosted build, state/geometry/font/calibration/localization/invalidation/energy-layout checks, exact-DLL tests and package boundaries passed. Accepted cases and limits are owned by [TEST_LOG.md](TEST_LOG.md). The live acceptance snapshot is `.local/evidence/accept-0.1.16-visual/LogOutput.log`, SHA256 `96ae57df373be5e2137e26f9e87678ddac37a51ecdccccde85c7a66570c5316d`: it loads 0.1.16 with no planner exception/disable found. It is a partial running-session snapshot, not a completed shutdown log.

## Next development step

Repeatable-placeable reduction is implemented in dev/0.1.20, verified for the exercised native0.1.20 cases after exact CI build/installation. The selected READY mechanism captures the original goal/definition/inventory/WGO at DoPlace entry, requires exactly one successful matching native RemoveItems call and normal completion with the captured WGO becoming just_built, then reduces1/removes0 and refreshes existing consumers at LateUpdate. Planner-only state tests verify key isolation, rejected/cancelled/error attempts, once-only receipts, stale membership, derived requirements and zero removal. Native code alone spends materials.

[PLACEMENT_COMMIT_RESEARCH.md](PLACEMENT_COMMIT_RESEARCH.md) owns the returned passive probe evidence and production gate. Reuse it; no probe reinstall is needed. Returned0.1.20 evidence confirms ready activation,9 once-only decrements, zero removal, unrelated goal preservation and4 busy failures without decrement; standalone cancellation and upstream resource refusal likewise add none. Actual material totals/rendering are not asserted from the log. Next required functional work is the separate upgrade/repair/clearing boundary investigation. Repairs/upgrades/clearing have separate unclosed success boundaries; do not infer completion from menu selection or arbitrary inventory changes.

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

Private `.local/compile-native.ps1` verifies against installed host references (default calibration, `-Production` without it). `.local/REFERENCE_BUILD_NOTE.md` records an unresolved local full-SDK/cache issue: stale reference outputs must not be treated as fresh success. Native-reference compilation and clean hosted builds are the established verification paths. Private reflection helpers, logs, proprietary inputs and machine installation configuration stay ignored; consult existing `.local/` evidence before recreating them. Previous0.1.19 DLL/log/receipt are preserved under private .local/evidence/pre-0.1.20/ and .local/installation-backups/pre-0.1.20/. Host was closed at installation. Returned production evidence is private under .local/evidence/return-0.1.20-placement/; exact hash and bounded findings are in TEST_LOG.md.
