# Development and recovery

Updated: 2026-10-18. This is the current resume entry point; dated design and test records describe their own candidate, not competing current instructions.

## Current state

Building Planner keeps stable technical identity CraftingPlanner. Visual baseline **0.1.19**, explicitly accepted, restores both user-tuned 0.1.18 layouts independently: materials without native costs29/0, with native costs46/0; common native cost24/-1, counter17/1 and fine-arrow X8/-9. Profile choice follows actual visible native cost-block presence per row, including mixed menus. Right increase arrows hide at repeatable20/one-off1 in menu and shared HUD, and return below the bound without moving the numeral.

F2 format10 retains60 individually resettable sliders. The user accepts0.1.19 independent settings,20/19 arrow behavior and visuals. TEST_LOG.md owns the bounded acceptance and log identity; no further visual tuning is requested. Ordinary HUD across all11 languages is user-reported from0.1.11; gamepad/all11-language functional acceptance remains open. This is not stable-release acceptance.

Installed dev/0.1.22 preserves accepted repeatable/world completion and visuals, adds one-time scripted builder upgrade removal and one-time Put removal. Exact CI/downloaded-byte/native-reference/state/absent-row/catalogue checks pass. New0.1.22 runtime acceptance remains pending. The returned0.1.21 repair/clearing pass remains bounded evidence; Max Buttons is deferred with unresolved ownership. Home callback furniture placement is separately excluded.

## Exact installed artifact

| Identity | Value |
| --- | --- |
| Development branch | `dev/0.1.22` |
| Frozen runtime source | `c5a48b283d5bf3a572ac49feb8abd8b968d65093` |
| Version | 0.1.22 / assembly0.1.22.0 |
| CI run / artifact | `39083281426` / `12119389208` |
| DLL SHA256 | `e26b74274b9a7157aab9e111442506abcfc117dcbfbacfe257a95e3c6ef2bdec` |
| ZIP SHA256 | `4098c91a8af03c7539421b54e036a8c040cfcc0941469838b30b718956638548` |
| Calibration | enabled; F2 format 10, 60 individually resettable sliders |

Exact CI bytes are retained under ignored `artifacts/CraftingPlanner-0.1.22/`; previous payloads and installation receipts remain private under `.local/`. Numbered artifacts are immutable. Later documentation commits do not change this frozen runtime identity. The0.1.19 acceptance snapshot is recorded in TEST_LOG.md; its hash is023e04bcdf165d7f9c69f78b2f336ae1ac9a9bd8460660766a456b772f5cb51a.

Both native-reference configurations, clean hosted build, state/geometry/font/calibration/localization/invalidation/energy-layout checks, exact-DLL tests and package boundaries passed. Accepted cases and limits are owned by [TEST_LOG.md](TEST_LOG.md). The live acceptance snapshot is `.local/evidence/accept-0.1.16-visual/LogOutput.log`, SHA256 `96ae57df373be5e2137e26f9e87678ddac37a51ecdccccde85c7a66570c5316d`: it loads 0.1.16 with no planner exception/disable found. It is a partial running-session snapshot, not a completed shutdown log.

## Next development step

Repeatable-placeable reduction is implemented in dev/0.1.20, verified for the exercised native0.1.20 cases after exact CI build/installation. The selected READY mechanism captures the original goal/definition/inventory/WGO at DoPlace entry, requires exactly one successful matching native RemoveItems call and normal completion with the captured WGO becoming just_built, then reduces1/removes0 and refreshes existing consumers at LateUpdate. Planner-only state tests verify key isolation, rejected/cancelled/error attempts, once-only receipts, stale membership, derived requirements and zero removal. Native code alone spends materials.

[PLACEMENT_COMMIT_RESEARCH.md](PLACEMENT_COMMIT_RESEARCH.md) owns the returned passive probe evidence and production gate. Reuse it; no probe reinstall is needed. Returned0.1.20 evidence confirms ready activation,9 once-only decrements, zero removal, unrelated goal preservation and4 busy failures without decrement; standalone cancellation and upstream resource refusal likewise add none. Actual material totals/rendering are not asserted from the log. Actual pinned repair/clearing removal and unrelated repair preservation now pass in0.1.21. The next required functional investigation is one-time builder upgrade completion; the existing passive OneOffCompletionProbe0.1.0 already supports that observation. Do not infer completion from menu selection or arbitrary inventory changes.

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

## Current world-completion and redraw work

User requests repair/clearing removal and the Max Buttons redraw fix, with upgrades separate. Installed0.1.21 uses normal scoped ProcessFinishedCraft + exactly one matching successful OnFinishedCraft and original captured goal identity. A Unity-null guard protects our Redraw prefix before GetComponent; the exact invalid-reference origin in the Max Buttons chain is not asserted. Both native-reference modes, hosted CI/state tests and real-metadata null/detached-prefix tests pass, including the exact downloaded DLL. Actual pinned clearing/repair removal and unrelated repair preservation are verified in the0.1.21 return; full Max Buttons gamepad compatibility remains open. ONEOFF_COMPLETION_RESEARCH.md owns two READY gates and acceptance bounds.

The passive OneOffCompletionProbe0.1.0 was hash-verified, backed up outside loading and removed at0.1.21 installation, with the game closed. Prior0.1.20 DLL/probe are private under .local/installation-backups/pre-0.1.21/; log/install receipt under .local/evidence/pre-0.1.21/. Its frozen artifact remains available for later builder-upgrade observation. TEST_LOG.md owns exact installed identities. Returned0.1.21 log identity and bounded acceptance are in TEST_LOG.md. No agent-triggered game launch or full release acceptance is asserted.

Current remaining defect: Max Buttons continues craft max/min input while TechTreeGUI is open after CraftGUI hiding. The9 new stacks have no explicit planner-prefix frame; do not describe the entire redraw issue as fixed or suppress native Redraw to conceal it. ONEOFF_COMPLETION_RESEARCH.md owns observed context, inspected input flag lifecycle and uncertainty about the exact null member.

User decision: defer Max Buttons investigation until the final compatibility/release pass. Ownership remains undetermined; the user notes the error only surfaced in this development period, so do not assert it predates Building Planner changes or belongs solely to a third party. A thread reminder was created for that milestone. Current next functional step is the separate one-time builder upgrade success test; no new runtime change or probe installation is part of this planning turn.

Yard research is complete for the observed native path as of2026-10-18. Exact0.1.0 observer/setup return confirms native yard builder/component owner/recipe, one successful material spend and synchronous nested ProcessFinishedCraft with one matching successful completion marker/End. No error in this chain; helper-issued materials were explicitly synthetic and the user ran the ordinary upgrade. ONEOFF_COMPLETION_RESEARCH.md owns the bounded READY gate and TEST_LOG.md exact evidence. Builder auto-removal is still absent in production0.1.21; next is its implementation/compiled tests and a candidate acceptance, not another boundary-discovery probe. Do not generalize to arbitrary one-time placement or scripted callback flows.

Both sidecars were verified/backed up and removed after return preservation with host closed; only production0.1.21 remains installed. Artifacts remain available for any needed acceptance setup. Private return snapshot SHA2561d696f78b8b921e694dd353e392d078a8d587cb75786fc71e2ee0c1fafe94ad1 and cleanup receipt are under .local/evidence/yard-upgrade-return/. No new production code/build or save/inventory change in this review. Max Buttons remains deferred until compatibility/release work.

Installed0.1.22 implements the observed yard completion family across39 equivalent authored one-time builder actions, plus17 one-time Put placements. Host flags route already eligible goals without recipe ID tables. All57 authored one-time definitions pass compiled dispatch checks (39/17/1 no-needs exclusion); existing state/geometry/native-absent-row regressions pass, both native-reference modes and clean CI pass. Actual0.1.22 goal removal still needs the user/native pass. Home furniture/decor uses separate callback placement and is not claimed covered.

Installed with host closed at2026-10-18T00:36:54Z after exact artifact verification; prior0.1.21 DLL/log and receipt are private under .local/installation-backups/pre-0.1.22/ and .local/evidence/pre-0.1.22/. Exact existing YardUpgradeSetup0.1.0 (F8) is temporarily restored to avoid resource gathering for acceptance on an unupgraded save; source5e14e3d, DLL051fcb8a4202a32b692f3b7a92fc289d11939670bc42b74c02f82b227fd41e30. Its synthetic resources/native upgrade may be saved; same existing undo/pre-test-save contract applies. Passive OneOffCompletionProbe stays absent. Remove resource helper after the return. No new game launch, runtime acceptance or stable promotion is claimed.

Next native pass: pin yard upgrade (or another available39-family action), leave another goal pinned, perform normally and confirm exactly one goal_completed/removal plus remaining goal/material refresh. If available exercise one-time Put (for example a soul totem) for exactly one goal_placed/removal and no decrement on cancellation/busy refusal. Do not ask for57 live tests; static corpus coverage is distinct from representative native acceptance. Shared native audit promoted to GraveyardKeeperResearche8eabd8f1eb812eb49de0a6296b1ecd5ed0c32bd. Max Buttons stays deferred; callback furniture requires its own later boundary, not instant-upgrade inclusion.
