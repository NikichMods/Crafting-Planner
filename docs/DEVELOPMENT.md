# Development and recovery

Updated: 2026-10-17. This is the current resume entry point; dated design and test records describe their own candidate, not competing current instructions.

## Current state

Building Planner keeps stable technical identity CraftingPlanner. Visual baseline **0.1.19**, explicitly accepted, restores both user-tuned 0.1.18 layouts independently: materials without native costs29/0, with native costs46/0; common native cost24/-1, counter17/1 and fine-arrow X8/-9. Profile choice follows actual visible native cost-block presence per row, including mixed menus. Right increase arrows hide at repeatable20/one-off1 in menu and shared HUD, and return below the bound without moving the numeral.

F2 format10 retains60 individually resettable sliders. The user accepts0.1.19 independent settings,20/19 arrow behavior and visuals. TEST_LOG.md owns the bounded acceptance and log identity; no further visual tuning is requested. Ordinary HUD across all11 languages is user-reported from0.1.11; gamepad/all11-language functional acceptance remains open. This is not stable-release acceptance.

Installed dev/0.1.21 preserves accepted repeatable placement and visuals, adds repair/clearing WorldChange removal and guards our row Redraw prefix against Unity-null. Exact CI/downloaded-byte/native-reference/state/absent-row checks pass; returned0.1.21 pinned repair/clearing removal and unrelated-goal preservation now pass; Max Buttons Redux input still fails with9 redraw exceptions during technology tabs. Builder upgrades stay excluded.

## Exact installed artifact

| Identity | Value |
| --- | --- |
| Development branch | `dev/0.1.21` |
| Frozen runtime source | `25fd3181091492b20735d52e1b2ccc16c09a7405` |
| Version | 0.1.21 / assembly0.1.21.0 |
| CI run / artifact | `39061707706` / `12111662358` |
| DLL SHA256 | `90177d2662daf51eb87858884ea8f93878b3cd9d90d993c1b52006fd7ed102da` |
| ZIP SHA256 | `4b5d445e78b4fb92197d221793ce5fc5d8318595fd00b37b8c708ba2234cb166` |
| Calibration | enabled; F2 format 10, 60 individually resettable sliders |

Exact CI bytes are retained under ignored `artifacts/CraftingPlanner-0.1.21/`; previous payloads and installation receipts remain private under `.local/`. Numbered artifacts are immutable. Later documentation commits do not change this frozen runtime identity. The0.1.19 acceptance snapshot is recorded in TEST_LOG.md; its hash is023e04bcdf165d7f9c69f78b2f336ae1ac9a9bd8460660766a456b772f5cb51a.

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

Active research/yard-upgrade-setup prepares a separate F8 missing-material helper beside unchanged0.1.21 and the existing exact passive one-off probe. The user authorized resource preparation to avoid gathering10 detail_2/8 wooden_plank/4 nails on an available unupgraded save. Read ONEOFF_COMPLETION_RESEARCH.md for static zero-time upgrade facts, native capacity/grant/cleanup safety and the one normal action required. This is research setup, not production upgrade removal; Max Buttons remains deferred. Exact artifact/install identities belong in TEST_LOG.md.
